// Dalax_BiobotProject/BlockchainIntegration/gen_metadata.js
require('dotenv').config(); // For loading environment variables like Pinata keys
const axios = require('axios'); // For making HTTP requests to Pinata
// FormData is not strictly needed if pinning JSON directly, but useful for files.
// If you were pinning an image file first, you'd use FormData.

const PINATA_API_KEY = process.env.PINATA_API_KEY;
const PINATA_SECRET_API_KEY = process.env.PINATA_SECRET_API_KEY;
const PINATA_JWT = process.env.PINATA_JWT; // Alternative: Pinata JWT

// --- Helper: Check Pinata Authentication ---
function checkPinataAuth() {
    // Prefer JWT if available
    if (PINATA_JWT && PINATA_JWT.length > 0) {
        return 'jwt';
    }
    if (PINATA_API_KEY && PINATA_SECRET_API_KEY) {
        return 'keys';
    }
    console.warn(
`[gen_metadata.js] Pinata API Key/Secret or JWT not configured in .env file. 
IPFS uploads will use placeholders. Please set PINATA_API_KEY & PINATA_SECRET_API_KEY or PINATA_JWT.`
    );
    return null;
}

// --- Metadata Generation Functions (from previous version - can be expanded) ---

function generateBiobotMetadataJSON(biobotData, imageUrl) {
    if (!biobotData || !biobotData.id || !biobotData.name) {
        throw new Error("[gen_metadata.js] Biobot data must include at least id and name.");
    }
    const attributes = [
        {
            "trait_type": "DNA Sequence Snippet",
            "value": biobotData.dna ? biobotData.dna.substring(0, 20) + "..." : "N/A"
        },
        { "trait_type": "Generation", "value": biobotData.generation || 1 },
        // Add more dynamic attributes from biobotData
        // e.g., { "trait_type": "Strength", "value": biobotData.strength || 0 },
    ];
    if (biobotData.customAttributes) { // Assuming customAttributes is an array of {trait_type: string, value: any}
        attributes.push(...biobotData.customAttributes);
    }
    return {
        "name": `${biobotData.name} - #${biobotData.id}`,
        "description": `A unique Biobot from the Dalax Project. Generation: ${biobotData.generation || 1}.`,
        "image": imageUrl, // e.g., "ipfs://QmYourImageHashForBiobot"
        "external_url": `https://yourproject.com/biobot/${biobotData.id}`, // Optional
        "attributes": attributes
    };
}

function generateGravastarMetadataJSON(gravastarData, imageUrl) {
    // ... (similar to previous version, ensure it's complete) ...
    if (!gravastarData || !gravastarData.id || !gravastarData.name) {
        throw new Error("[gen_metadata.js] Gravastar data must include at least id and name.");
    }
    return {
        "name": `${gravastarData.name} - #${gravastarData.id}`,
        "description": "A quantum-anchored gravitational core node from the Dalax Project.",
        "image": imageUrl, // e.g., "ipfs://QmYourImageHashForGravastar"
        "attributes": [
            { "trait_type": "Stability", "value": gravastarData.stability || 0 },
            { "trait_type": "Intensity", "value": gravastarData.intensity || 0 },
            { "trait_type": "Energy Level", "value": gravastarData.energyLevel || 0 },
            { "trait_type": "Temporal Anchor", "value": gravastarData.temporalAnchor || false }
        ]
    };
}


/**
 * Uploads JSON metadata to IPFS using Pinata's 'pinJSONToIPFS' endpoint.
 * @param {object} jsonData - The JSON object to upload.
 * @param {string} pinataFileName - The desired filename for the pinned content on Pinata.
 * @returns {Promise<string>} The IPFS hash (CID) of the uploaded JSON (e.g., "Qm...")
 */
async function pinJsonToIpfsWithPinata(jsonData, pinataFileName) {
    const authType = checkPinataAuth();
    if (!authType) {
        // Return a placeholder if auth is not set, allowing frontend to proceed with a non-real URI for testing
        const placeholderHash = `placeholder_${pinataFileName.replace(/[^a-zA-Z0-9]/g, "")}_${Date.now()}`;
        console.log(`[gen_metadata.js] (Placeholder) IPFS URI: ipfs://${placeholderHash}`);
        return `ipfs://${placeholderHash}`;
    }

    const url = `https://api.pinata.cloud/pinning/pinJSONToIPFS`;
    let headers;

    if (authType === 'jwt') {
        headers = { 'Authorization': `Bearer ${PINATA_JWT}` };
    } else { // keys
        headers = {
            'pinata_api_key': PINATA_API_KEY,
            'pinata_secret_api_key': PINATA_SECRET_API_KEY
        };
    }
    headers['Content-Type'] = 'application/json';

    // The body for pinJSONToIPFS should be the JSON object directly,
    // or wrapped if you want to specify pinataOptions or pinataMetadata.
    const body = {
        pinataOptions: {
            cidVersion: 1 // Use CIDv1 for better compatibility
        },
        pinataMetadata: {
            name: pinataFileName, // Filename as it appears on Pinata
            keyvalues: { // Optional custom keyvalues for Pinata dashboard filtering
                project: 'Dalax_BiobotProject',
                type: jsonData.attributes ? jsonData.attributes[0].value : 'metadata' // Example
            }
        },
        pinataContent: jsonData // The actual JSON data to pin
    };

    console.log(`[gen_metadata.js] Pinning '${pinataFileName}' to IPFS via Pinata...`);

    try {
        const response = await axios.post(url, body, { headers: headers });
        if (response.data.IpfsHash) {
            console.log(`[gen_metadata.js] Successfully pinned JSON to IPFS. CID: ${response.data.IpfsHash}`);
            return `ipfs://${response.data.IpfsHash}`;
        } else {
            throw new Error("[gen_metadata.js] Pinata response did not include IpfsHash.");
        }
    } catch (error) {
        const errorMessage = error.response ? JSON.stringify(error.response.data) : error.message;
        console.error(`[gen_metadata.js] Error uploading JSON to Pinata: ${errorMessage}`);
        // Depending on your error strategy, you might want to re-throw or return a specific error object.
        throw new Error(`[gen_metadata.js] Failed to upload JSON metadata to IPFS: ${errorMessage}`);
    }
}

/**
 * Main function to generate metadata for an entity (Biobot or Gravastar)
 * and pin it to IPFS.
 * @param {object} entityData - The data object for the entity.
 * @param {string} entityType - A string like 'biobot' or 'gravastar'.
 * @param {string} entityImageUrl - The IPFS URI of the entity's image (e.g., "ipfs://QmImageHash").
 * This image should be pinned to IPFS *before* calling this function.
 * @returns {Promise<string>} The IPFS URI for the generated and pinned metadata JSON.
 */
async function generateAndPinMetadata(entityData, entityType, entityImageUrl) {
    if (!entityData || !entityType || !entityImageUrl) {
        throw new Error("[gen_metadata.js] Missing entityData, entityType, or entityImageUrl for metadata generation.");
    }
    if (!entityImageUrl.startsWith('ipfs://')) {
        console.warn(`[gen_metadata.js] entityImageUrl "${entityImageUrl}" does not start with ipfs://. Ensure it's a valid IPFS URI.`);
    }


    let metadataJson;
    let pinataFileName;
    const entityId = entityData.id || Date.now(); // Use entity ID or timestamp for filename

    if (entityType.toLowerCase() === 'biobot') {
        metadataJson = generateBiobotMetadataJSON(entityData, entityImageUrl);
        pinataFileName = `biobot_${entityId}_metadata.json`;
    } else if (entityType.toLowerCase() === 'gravastar') {
        metadataJson = generateGravastarMetadataJSON(entityData, entityImageUrl);
        pinataFileName = `gravastar_${entityId}_metadata.json`;
    } else {
        throw new Error(`[gen_metadata.js] Unsupported entity type for metadata generation: ${entityType}`);
    }

    console.log(`[gen_metadata.js] Generated metadata for ${entityType} ID ${entityId}:`, metadataJson);
    const metadataIpfsUri = await pinJsonToIpfsWithPinata(metadataJson, pinataFileName);
    return metadataIpfsUri;
}

// To use this module in mint_api.js:
// const { generateAndPinMetadata } = require('./gen_metadata');
//
// Example usage within mint_api.js:
// try {
//   const imageIpfsUri = "ipfs://QmExistingImageHash"; // Assume image is already on IPFS
//   const biobotData = { id: 123, name: "CyberBot", dna: "ATCG...", strength: 50 };
//   const metadataUri = await generateAndPinMetadata(biobotData, 'biobot', imageIpfsUri);
//   console.log('NFT Metadata URI:', metadataUri); // This URI goes into your NFT
// } catch (error) {
//   console.error('Failed to process metadata:', error);
// }

module.exports = {
    generateAndPinMetadata,
    // You can also export the individual generation functions if needed elsewhere
    generateBiobotMetadataJSON,
    generateGravastarMetadataJSON,
    pinJsonToIpfsWithPinata // Potentially useful if you need to pin other JSON data
};

// To test this file directly (e.g., `node gen_metadata.js` from BlockchainIntegration folder):
// async function testRun() {
//     if (require.main !== module) return; // Don't run if imported

//     console.log("[gen_metadata.js] Running direct test...");
//     // Ensure you have a .env file in the root of Dalax_BiobotProject with Pinata keys/JWT
//     // or the test will use placeholder IPFS URIs.

//     const sampleBiobotData = { id: "test001", name: "Testy Bot", dna: "ATCG...", generation: 1, customAttributes: [{trait_type: "Color", value: "Blue"}] };
//     const sampleBiobotImageUri = "ipfs://QmPlaceholderImageHashBiobot"; 
    
//     const sampleGravastarData = { id: "testG001", name: "Testy Star", stability: 0.99, intensity: 8.5, energyLevel: 250, temporalAnchor: true };
//     const sampleGravastarImageUri = "ipfs://QmPlaceholderImageHashGravastar";

//     try {
//         console.log("\nTesting Biobot Metadata Generation & Pinning:");
//         const biobotMetadataUri = await generateAndPinMetadata(sampleBiobotData, 'biobot', sampleBiobotImageUri);
//         console.log("==> Biobot Metadata IPFS URI:", biobotMetadataUri);

//         console.log("\nTesting Gravastar Metadata Generation & Pinning:");
//         const gravastarMetadataUri = await generateAndPinMetadata(sampleGravastarData, 'gravastar', sampleGravastarImageUri);
//         console.log("==> Gravastar Metadata IPFS URI:", gravastarMetadataUri);
//     } catch (error) {
//         console.error("\n[gen_metadata.js] Test Run Error:", error.message);
//     }
// }
// testRun();
