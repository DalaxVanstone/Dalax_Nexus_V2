// Dalax_BiobotProject/BlockchainIntegration/mint_api.js
require('dotenv').config({ path: require('path').resolve(__dirname, '../../.env') });
const express = require('express');
const { ethers } = require('ethers');
const cors = require('cors');
const fs = require('fs');
const path = require('path');
const axios = require('axios');
const FormData = require('form-data');
const multer = require('multer');
const { v4: uuidv4 } = require('uuid'); // For generating unique filenames

// Import metadata generation and pinning logic from gen_metadata.js
// Ensure gen_metadata.js is in the same directory or provide correct path
const { generateAndPinMetadata } = require('./gen_metadata'); 

const app = express();
const port = process.env.MINT_API_PORT || 3001;

const LOG_PREFIX = "[MINT_API_SERVER]"; // For structured logging

// --- Configuration & Environment Variable Checks ---
const biobotNFTContractAddress = process.env.BIOBOT_NFT_CONTRACT_ADDRESS;
const gravastarNFTContractAddress = process.env.GRAVASTAR_NFT_CONTRACT_ADDRESS;
const ethNodeUrl = process.env.RPC_URL_LOCALHOST;
const minterPrivateKey = process.env.MINT_API_WALLET_PRIVATE_KEY;

const PINATA_API_KEY = process.env.PINATA_API_KEY;
const PINATA_SECRET_API_KEY = process.env.PINATA_SECRET_API_KEY;
const PINATA_JWT = process.env.PINATA_JWT;

if (!ethNodeUrl || !minterPrivateKey) {
    console.error(`${LOG_PREFIX} CRITICAL: Missing ETH_NODE_URL or MINTER_PRIVATE_KEY in .env file.`);
    process.exit(1);
}
// ... (other config checks from your uploaded file) ...

// --- Ethers.js Setup ---
const provider = new ethers.providers.JsonRpcProvider(ethNodeUrl);
const wallet = new ethers.Wallet(minterPrivateKey, provider);
console.log(`${LOG_PREFIX} Minter wallet address configured: ${wallet.address}`);

// --- Load Contract ABIs ---
function loadContractAbi(contractName) {
    try {
        const abiPath = path.resolve(__dirname, `./artifacts/contracts/${contractName}.sol/${contractName}.json`);
        if (fs.existsSync(abiPath)) {
            const contractJson = JSON.parse(fs.readFileSync(abiPath, 'utf8'));
            console.log(`${LOG_PREFIX} Successfully loaded ABI for ${contractName}`);
            return contractJson.abi;
        } else {
            console.warn(`${LOG_PREFIX} ABI file not found for ${contractName} at ${abiPath}`);
            return null;
        }
    } catch (error) {
        console.error(`${LOG_PREFIX} Error loading ABI for ${contractName}:`, error);
        return null;
    }
} //

const biobotNFTAbi = loadContractAbi("BiobotNFT"); //
const gravastarNFTAbi = loadContractAbi("GravastarNFT"); //

// --- Contract Instances ---
let biobotNftContract;
if (biobotNFTContractAddress && biobotNFTAbi) {
    biobotNftContract = new ethers.Contract(biobotNFTContractAddress, biobotNFTAbi, wallet); //
    console.log(`${LOG_PREFIX} BiobotNFT contract initialized at ${biobotNFTContractAddress}`); //
} else {
    console.warn(`${LOG_PREFIX} BiobotNFT contract could not be initialized. Check address and ABI in .env file.`); //
}

let gravastarNftContract;
if (gravastarNFTContractAddress && gravastarNFTAbi) {
    gravastarNftContract = new ethers.Contract(gravastarNFTContractAddress, gravastarNFTAbi, wallet); //
    console.log(`${LOG_PREFIX} GravastarNFT contract initialized at ${gravastarNFTContractAddress}`); //
} else {
    console.warn(`${LOG_PREFIX} GravastarNFT contract could not be initialized. Check address and ABI in .env file.`); //
}

// --- Pinata Authentication Helper ---
function getPinataAuthHeaders() {
    if (PINATA_JWT && PINATA_JWT.length > 0) return { 'Authorization': `Bearer ${PINATA_JWT}` }; //
    if (PINATA_API_KEY && PINATA_SECRET_API_KEY) return { //
        'pinata_api_key': PINATA_API_KEY, //
        'pinata_secret_api_key': PINATA_SECRET_API_KEY //
    };
    console.warn(`${LOG_PREFIX} Pinata API Key/Secret or JWT not configured. IPFS uploads will use placeholders or fail.`); //
    return null;
} //

// --- IPFS Image Upload Function (using Pinata) ---
async function pinFileToIpfsWithPinata(imageFilePath, originalFileNameForPinata) {
    const authHeaders = getPinataAuthHeaders(); //
    if (!authHeaders) {
        const placeholderHash = `placeholder_img_${originalFileNameForPinata.replace(/[^a-zA-Z0-9]/g, "")}_${Date.now()}`; //
        console.log(`${LOG_PREFIX} (Placeholder) Image IPFS URI: ipfs://${placeholderHash}`); //
        return `ipfs://${placeholderHash}`; //
    }

    if (!fs.existsSync(imageFilePath)) { //
        console.error(`${LOG_PREFIX} Image file not found at path: ${imageFilePath}`);
        throw new Error(`Image file not found at path: ${imageFilePath}`);
    }

    const url = `https://api.pinata.cloud/pinning/pinFileToIPFS`; //
    const data = new FormData(); //
    data.append('file', fs.createReadStream(imageFilePath), { filename: originalFileNameForPinata }); //
    data.append('pinataOptions', JSON.stringify({ cidVersion: 1 })); //
    data.append('pinataMetadata', JSON.stringify({ //
        name: originalFileNameForPinata, //
        keyvalues: { project: 'Dalax_BiobotProject', type: 'nft_image' } //
    }));

    console.log(`${LOG_PREFIX} Pinning image '${originalFileNameForPinata}' from '${imageFilePath}' to IPFS via Pinata...`); //
    try {
        const response = await axios.post(url, data, { //
            maxBodyLength: Infinity, //
            headers: { //
                ...authHeaders, //
                'Content-Type': `multipart/form-data; boundary=${data._boundary}`, //
            }
        });
        if (response.data.IpfsHash) { //
            console.log(`${LOG_PREFIX} Successfully pinned image to IPFS. CID: ${response.data.IpfsHash}`); //
            return `ipfs://${response.data.IpfsHash}`; //
        } else {
            throw new Error("Pinata image pinning response did not include IpfsHash."); //
        }
    } catch (error) {
        const errorMessage = error.response ? JSON.stringify(error.response.data) : error.message; //
        console.error(`${LOG_PREFIX} Error uploading image to Pinata: ${errorMessage}`); //
        throw new Error(`Failed to upload image to IPFS: ${errorMessage}`); //
    }
} //

// --- Multer Configuration for File Uploads ---
const UPLOADS_DIR = path.resolve(__dirname, 'uploads');
if (!fs.existsSync(UPLOADS_DIR)) {
    try {
        fs.mkdirSync(UPLOADS_DIR, { recursive: true });
        console.log(`${LOG_PREFIX} Created uploads directory at: ${UPLOADS_DIR}`);
    } catch (err) {
        console.error(`${LOG_PREFIX} Failed to create uploads directory at ${UPLOADS_DIR}:`, err);
        // Consider exiting if uploads are critical and directory can't be made
        // process.exit(1); 
    }
}

const storage = multer.diskStorage({
    destination: function (req, file, cb) {
        cb(null, UPLOADS_DIR);
    },
    filename: function (req, file, cb) {
        // Generate a unique filename using UUID to avoid collisions and hide original filenames
        const uniqueSuffix = uuidv4();
        const extension = path.extname(file.originalname).toLowerCase();
        cb(null, `${uniqueSuffix}${extension}`);
    }
});

const fileFilter = (req, file, cb) => {
    const allowedTypes = ['.png', '.jpg', '.jpeg', '.gif'];
    const extension = path.extname(file.originalname).toLowerCase();
    if (allowedTypes.includes(extension)) {
        cb(null, true);
    } else {
        cb(new Error('Invalid file type. Only PNG, JPG, JPEG, and GIF images are allowed.'), false);
    }
};

const upload = multer({ 
    storage: storage,
    limits: { fileSize: 10 * 1024 * 1024 }, // Limit file size to 10MB
    fileFilter: fileFilter 
});

// --- Middleware ---
app.use(cors()); //
app.use(express.json({ limit: '10mb' })); //
app.use(express.urlencoded({ extended: true, limit: '10mb' })); //

// --- API Routes ---

app.get('/', (req, res) => {
    res.send('Dalax Biobot Minting API is Active and Ready!'); //
});

/**
 * @route POST /upload/image
 * @desc Uploads an image file. The client will then use the returned 'serverFileName'
 * in subsequent calls to the minting endpoints.
 * @body { multipart/form-data; fieldname: 'nftImage' }
 */
app.post('/upload/image', upload.single('nftImage'), (req, res) => {
    if (!req.file) {
        console.warn(`${LOG_PREFIX} Image upload attempt failed: No file provided.`);
        return res.status(400).json({ success: false, error: 'No image file uploaded.' });
    }
    console.log(`${LOG_PREFIX} Image uploaded successfully and saved on server: ${req.file.filename} (Original: ${req.file.originalname})`);
    res.status(201).json({
        success: true,
        message: 'Image uploaded successfully. Use this serverFileName in your mint request.',
        serverFileName: req.file.filename, // This is the unique filename generated by multer
        originalFileName: req.file.originalname,
        // Storing the path is more for server-side reference, client only needs serverFileName
    });
}, (error, req, res, next) => { // Custom error handler specifically for multer errors
    if (error instanceof multer.MulterError) {
        console.warn(`${LOG_PREFIX} Multer error during image upload: ${error.message}`);
        return res.status(400).json({ success: false, error: `Image upload error: ${error.message}. Check file size or type.` });
    } else if (error) {
        console.warn(`${LOG_PREFIX} Non-multer error during image upload: ${error.message}`);
        return res.status(400).json({ success: false, error: error.message });
    }
    next();
});

/**
 * Generic Minting Function
 */
async function handleMintRequest(req, res, entityType, nftContract) {
    const { recipientAddress, entityData, serverImageFileName } = req.body; // Expecting serverImageFileName now

    // --- Input Validation ---
    if (!recipientAddress || !entityData || !serverImageFileName) { //
        console.warn(`${LOG_PREFIX} Mint request validation failed: Missing fields.`);
        return res.status(400).json({ success: false, error: 'Missing recipientAddress, entityData, or serverImageFileName in request body.' });
    }
    if (!ethers.utils.isAddress(recipientAddress)) { //
        console.warn(`${LOG_PREFIX} Mint request validation failed: Invalid recipient address ${recipientAddress}.`);
        return res.status(400).json({ success: false, error: 'Invalid recipient address.' });
    }
    if (typeof entityData !== 'object' || entityData === null || !entityData.id || !entityData.name) {
        console.warn(`${LOG_PREFIX} Mint request validation failed: entityData is invalid or missing id/name.`);
        return res.status(400).json({ success: false, error: 'entityData must be an object with at least id and name fields.'});
    }
    if (typeof serverImageFileName !== 'string' || serverImageFileName.includes('/') || serverImageFileName.includes('..')) {
        console.warn(`${LOG_PREFIX} Mint request validation failed: serverImageFileName is invalid.`);
        return res.status(400).json({ success: false, error: 'Invalid serverImageFileName.' });
    }
    if (!nftContract) { //
        console.error(`${LOG_PREFIX} Minting blocked: ${entityType}NFT contract not initialized.`);
        return res.status(500).json({ success: false, error: `${entityType}NFT contract not initialized on server. Check address and ABI in .env.` });
    }

    const localImageFilePath = path.join(UPLOADS_DIR, serverImageFileName);
    if (!fs.existsSync(localImageFilePath)) { //
        console.warn(`${LOG_PREFIX} Mint request failed: Image file '${serverImageFileName}' not found in uploads directory. Upload it first via /upload/image.`);
        return res.status(400).json({ success: false, error: `Image file '${serverImageFileName}' not found. Please upload it first via /upload/image.` });
    }

    console.log(`${LOG_PREFIX} Processing mint request for ${entityType}:`, { recipientAddress, entityData: {name: entityData.name, id: entityData.id}, serverImageFileName });
    OnMintingStatusUpdate(`[${entityType}:${entityData.id}] Processing mint request for ${recipientAddress}.`);


    try {
        OnMintingStatusUpdate(`[${entityType}:${entityData.id}] Pinning image ${serverImageFileName} to IPFS...`);
        const imageIpfsUri = await pinFileToIpfsWithPinata(localImageFilePath, serverImageFileName); // Use the serverImageFileName as Pinata filename too
        console.log(`${LOG_PREFIX} Image pinned to IPFS: ${imageIpfsUri}`);
        OnMintingStatusUpdate(`[${entityType}:${entityData.id}] Image pinned: ${imageIpfsUri}. Generating metadata...`);
        
        const metadataUri = await generateAndPinMetadata(entityData, entityType, imageIpfsUri); //
        console.log(`${LOG_PREFIX} Metadata generated and pinned to IPFS: ${metadataUri}`);
        OnMintingStatusUpdate(`[${entityType}:${entityData.id}] Metadata pinned: ${metadataUri}. Submitting transaction...`);

        // --- Transaction Options (Gas Price, Nonce - Conceptual) ---
        // For production, you might want to manage gas prices and nonces more explicitly.
        // const gasPrice = await provider.getGasPrice();
        // const nonce = await wallet.getTransactionCount("latest"); // Or manage nonces more carefully for concurrency
        // const txOptions = {
        //     gasPrice: gasPrice.mul(ethers.BigNumber.from(12)).div(10), // Example: 20% higher gas price
        //     nonce: nonce,
        //     // gasLimit: ethers.utils.hexlify(250000) // Estimate or set a specific limit
        // };
        
        let transaction;
        console.log(`${LOG_PREFIX} Calling smart contract mint function for ${entityType}...`);
        if (entityType === 'biobot' && nftContract.mintBiobot) { //
            transaction = await nftContract.mintBiobot(recipientAddress, metadataUri /*, txOptions */); //
        } else if (entityType === 'gravastar' && nftContract.mint) { // Assuming GravastarNFT uses a simple 'mint' //
            transaction = await nftContract.mint(recipientAddress, metadataUri /*, txOptions */); //
        } else {
            throw new Error(`No suitable mint function found on contract for type ${entityType}`); //
        }
        
        console.log(`${LOG_PREFIX} Mint transaction sent: ${transaction.hash}. Waiting for confirmation...`); //
        OnMintingStatusUpdate(`[${entityType}:${entityData.id}] Transaction sent: ${transaction.hash}. Waiting for confirmation...`);
        const receipt = await transaction.wait(); //
        console.log(`${LOG_PREFIX} Transaction confirmed. Gas used: ${receipt.gasUsed.toString()}`); //

        let tokenId = null; //
        const eventName = entityType === 'biobot' ? 'BiobotMinted' : 'Transfer'; //
        const mintEvent = receipt.events?.find(e => e.event === eventName); //
        
        if (mintEvent && mintEvent.args) { //
            tokenId = mintEvent.args.tokenId ? mintEvent.args.tokenId.toString() : (mintEvent.args[2] ? mintEvent.args[2].toString() : null); //
        }
         if (!tokenId && receipt.logs && receipt.logs.length > 0) { // Fallback for generic Transfer event
            const erc721TransferTopic = ethers.utils.id("Transfer(address,address,uint256)"); //
            for (const log of receipt.logs) { //
                if (log.topics[0] === erc721TransferTopic && log.topics.length === 4) { //
                    tokenId = ethers.BigNumber.from(log.topics[3]).toString(); //
                    break; //
                }
            }
        }
        if (!tokenId) console.warn(`${LOG_PREFIX} Could not determine Token ID from transaction receipt events.`); //
        
        OnMintingStatusUpdate(`[${entityType}:${entityData.id}] NFT Minted! Token ID: ${tokenId}, Tx: ${transaction.hash}.`);
        console.log(`${LOG_PREFIX} ${entityType} NFT Minted! Token ID: ${tokenId}, Tx: ${transaction.hash}, Metadata: ${metadataUri}, Image: ${imageIpfsUri}`);
        res.status(201).json({  //
            success: true, 
            message: `${entityType} NFT minted successfully!`, 
            transactionHash: transaction.hash,
            tokenId: tokenId,
            metadataUri: metadataUri,
            imageUri: imageIpfsUri
        });

    } catch (error) {
        console.error(`${LOG_PREFIX} Error during full minting process for ${entityType} ${entityData.id || ''}:`, error.message);
        OnMintingStatusUpdate(`[${entityType}:${entityData.id || ''}] Minting Error: ${error.message}`);
        res.status(500).json({ success: false, error: `Failed to mint ${entityType} NFT.`, details: error.message }); //
    }
} //

// --- Event Emitter (Conceptual, from your uploaded file) ---
const EventEmitter = require('events'); //
class MintingEmitter extends EventEmitter {} //
const mintingEmitter = new MintingEmitter(); //

function OnMintingStatusUpdate(message) { //
    console.log(`${LOG_PREFIX} STATUS_UPDATE: ${message}`); //
    mintingEmitter.emit('statusUpdate', message); //
}

app.post('/mint/biobot', (req, res) => { //
    handleMintRequest(req, res, 'biobot', biobotNftContract); //
});

app.post('/mint/gravastar', (req, res) => { //
    handleMintRequest(req, res, 'gravastar', gravastarNftContract); //
});

// --- Start Server ---
const server = app.listen(port, () => { // (Modified to assign to 'server')
    console.log(`${LOG_PREFIX} Minting API server now running at http://localhost:${port}`); //
    console.log(`${LOG_PREFIX} Uploads directory is: ${UPLOADS_DIR}`);
    console.log(`${LOG_PREFIX} Connected to Ethereum node at: ${ethNodeUrl}`); //
    if (biobotNFTContractAddress) console.log(`${LOG_PREFIX} BiobotNFT Contract: ${biobotNFTContractAddress}`); //
    if (gravastarNFTContractAddress) console.log(`${LOG_PREFIX} GravastarNFT Contract: ${gravastarNFTContractAddress}`); //
});

// For testing purposes (e.g. with Jest/Supertest)
module.exports = { app, server };
