// Dalax_BiobotProject/BlockchainIntegration/test/mint_api.test.js
const request = require('supertest');
const fs = require('fs-extra'); // For easier directory management
const path = require('path');
const { ethers } = require('ethers'); // For mocking BigNumber if needed by contract event args
const FormData = require('form-data'); // For verifying axios call with FormData for Pinata

// Import app and server from mint_api.js
// Ensure mint_api.js exports: module.exports = { app, server, mintingEmitter (optional) };
// Adjust path if your test file is located elsewhere relative to mint_api.js.
const { app, server } = require('../mint_api');

// --- Mocking Dependencies ---
jest.mock('../gen_metadata', () => ({
    generateAndPinMetadata: jest.fn(),
}));
const { generateAndPinMetadata } = require('../gen_metadata');

jest.mock('axios'); // Mock axios for Pinata calls
const axios = require('axios');

const mockMintBiobot = jest.fn();
const mockMintGravastar = jest.fn(); // Placeholder for Gravastar tests

jest.mock('ethers', () => {
    const originalEthers = jest.requireActual('ethers');
    return {
        ...originalEthers,
        Contract: jest.fn().mockImplementation((address, abi, signer) => {
            if (address === process.env.BIOBOT_NFT_CONTRACT_ADDRESS) {
                return { mintBiobot: mockMintBiobot };
            }
            if (address === process.env.GRAVASTAR_NFT_CONTRACT_ADDRESS) {
                return { mint: mockMintGravastar }; // Assuming 'mint' for Gravastar
            }
            console.warn(`Mock ethers.Contract called with unmocked address: ${address}`);
            return {}; // Default empty mock for any other contract address
        }),
        utils: { // Mock specific utils if your code uses them directly and they need mocking
            ...originalEthers.utils,
        },
        BigNumber: originalEthers.BigNumber, // Use actual BigNumber for consistency in mock data
    };
});

const UPLOADS_DIR = path.resolve(__dirname, '../uploads'); // Should match UPLOADS_DIR in mint_api.js
const TEST_RESOURCES_DIR = path.join(__dirname, 'test_resources_temp'); // Temp dir for test files created by tests

describe('Mint API Endpoints', () => {

    beforeAll(async () => {
        // Ensure uploads and temporary test_resources directories exist and are clean
        await fs.ensureDir(UPLOADS_DIR);
        await fs.emptyDir(UPLOADS_DIR); // Clean uploads before all tests
        await fs.ensureDir(TEST_RESOURCES_DIR);
        await fs.emptyDir(TEST_RESOURCES_DIR); // Clean test_resources before all tests
    });

    afterEach(async () => {
        // Clear mocks and the uploads directory after each individual test
        jest.clearAllMocks();
        await fs.emptyDir(UPLOADS_DIR);
    });

    afterAll(async (done) => {
        // Clean up the temporary test_resources directory after all tests are done
        await fs.remove(TEST_RESOURCES_DIR); // fs-extra's remove will delete the directory and its contents

        if (server && server.listening) {
            server.close(err => {
                if (err) {
                    console.error("Error closing server during test teardown:", err);
                    return done(err);
                }
                done();
            });
        } else {
            done();
        }
    });

    // --- Tests for POST /upload/image ---
    describe('POST /upload/image', () => {
        const testImageFileName = 'test-image.png';
        const testImagePath = path.join(TEST_RESOURCES_DIR, testImageFileName);
        const testTextFileName = 'test-file.txt';
        const testTextFilePath = path.join(TEST_RESOURCES_DIR, testTextFileName);

        beforeAll(async () => { // Create these specific test files once for this describe block
            await fs.writeFile(testImagePath, Buffer.from('fake PNG content for upload test'));
            await fs.writeFile(testTextFilePath, 'this is not an image file, it is a text file.');
        });

        it('should successfully upload a valid image file (PNG)', async () => {
            const response = await request(app)
                .post('/upload/image')
                .attach('nftImage', testImagePath); // 'nftImage' is the field name from multer

            expect(response.statusCode).toBe(201);
            expect(response.body.success).toBe(true);
            expect(response.body).toHaveProperty('message', 'Image uploaded successfully.');
            expect(response.body).toHaveProperty('serverFileName');
            expect(response.body.serverFileName).toMatch(new RegExp(`^[0-9a-fA-F-]{36}\\.png$`)); // UUIDv4 then .png
            expect(response.body).toHaveProperty('originalFileName', testImageFileName);

            const uploadedFilePath = path.join(UPLOADS_DIR, response.body.serverFileName);
            expect(await fs.pathExists(uploadedFilePath)).toBe(true);
            // const uploadedContent = await fs.readFile(uploadedFilePath);
            // const originalContent = await fs.readFile(testImagePath);
            // expect(uploadedContent).toEqual(originalContent); // Verify content if critical
        });

        it('should return 400 Bad Request if no image file is provided in the request', async () => {
            const response = await request(app)
                .post('/upload/image'); // No file attached

            expect(response.statusCode).toBe(400);
            expect(response.body.success).toBe(false);
            expect(response.body.error).toBe('No image file uploaded.');
            expect((await fs.readdir(UPLOADS_DIR)).length).toBe(0);
        });

        it('should return 400 Bad Request for an invalid file type (e.g., .txt)', async () => {
            const response = await request(app)
                .post('/upload/image')
                .attach('nftImage', testTextFilePath);

            expect(response.statusCode).toBe(400);
            expect(response.body.success).toBe(false);
            expect(response.body.error).toContain('Invalid file type. Only .png, .jpg, .jpeg, .gif allowed.');
            expect((await fs.readdir(UPLOADS_DIR)).length).toBe(0);
        });
    });

    // --- Tests for POST /mint/biobot ---
    describe('POST /mint/biobot', () => {
        const preUploadedImageFileName = 'biobot-test-img.png';
        const preUploadedImagePath = path.join(UPLOADS_DIR, preUploadedImageFileName);

        const validMintPayload = {
            recipientAddress: '0x1234567890123456789012345678901234567890',
            entityData: { name: 'TestBiobot', id: 'b001', dna: 'ATCGATCG', generation: 1 },
            serverImageFileName: preUploadedImageFileName,
        };

        beforeEach(async () => {
            // Create a dummy image file in UPLOADS_DIR to simulate it being pre-uploaded
            await fs.writeFile(preUploadedImagePath, Buffer.from('simulated pre-uploaded PNG content'));
        });

        it('should successfully mint a Biobot NFT with valid data and mocks', async () => {
            const mockIpfsImageHash = 'QmSuccessfullyPinnedImageHash';
            const mockIpfsMetadataURI = 'ipfs://QmSuccessfullyPinnedMetadataHash';
            const mockTxHash = '0xMockSuccessfulTransactionHashForBiobot';
            const mockTokenId = ethers.BigNumber.from(1); // Use ethers.BigNumber for mock

            axios.post.mockResolvedValueOnce({ data: { IpfsHash: mockIpfsImageHash } });
            generateAndPinMetadata.mockResolvedValueOnce(mockIpfsMetadataURI);
            mockMintBiobot.mockResolvedValueOnce({
                hash: mockTxHash,
                wait: jest.fn().mockResolvedValueOnce({
                    status: 1,
                    transactionHash: mockTxHash,
                    events: [{
                        event: 'BiobotMinted',
                        args: {
                            recipient: validMintPayload.recipientAddress,
                            tokenId: mockTokenId,
                            tokenURI: mockIpfsMetadataURI,
                        }
                    }]
                })
            });

            const response = await request(app)
                .post('/mint/biobot')
                .send(validMintPayload);

            expect(response.statusCode).toBe(201);
            expect(response.body.success).toBe(true);
            expect(response.body.message).toContain('Biobot NFT minted successfully!');
            expect(response.body.transactionHash).toBe(mockTxHash);
            expect(response.body.tokenId).toBe(mockTokenId.toString());
            expect(response.body.metadataUri).toBe(mockIpfsMetadataURI);
            expect(response.body.imageUri).toBe(`ipfs://${mockIpfsImageHash}`);

            expect(axios.post).toHaveBeenCalledWith(
                expect.stringContaining('api.pinata.cloud/pinning/pinFileToIPFS'),
                expect.any(FormData), // Check if FormData is passed
                expect.objectContaining({ headers: expect.objectContaining({'Authorization': `Bearer ${process.env.PINATA_JWT}`}) })
            );
            expect(generateAndPinMetadata).toHaveBeenCalledWith(
                validMintPayload.entityData,
                'biobot',
                `ipfs://${mockIpfsImageHash}`
            );
            expect(mockMintBiobot).toHaveBeenCalledWith(
                validMintPayload.recipientAddress,
                mockIpfsMetadataURI
            );
        });

        it('should return 400 if serverImageFileName is missing from payload', async () => {
            const { serverImageFileName, ...payload } = validMintPayload;
            const response = await request(app).post('/mint/biobot').send(payload);
            expect(response.statusCode).toBe(400);
            expect(response.body.error).toMatch(/Missing recipientAddress, entityData, or serverImageFileName/i);
        });

        it('should return 400 if image specified by serverImageFileName does not exist in uploads', async () => {
            const payload = { ...validMintPayload, serverImageFileName: 'this-file-does-not-exist.png' };
            const response = await request(app).post('/mint/biobot').send(payload);
            expect(response.statusCode).toBe(400);
            expect(response.body.error).toContain("Image file 'this-file-does-not-exist.png' not found in uploads directory.");
        });

        it('should return 500 if Pinata image pinning (axios.post) fails', async () => {
            axios.post.mockRejectedValueOnce(new Error('Simulated Pinata API Network Error'));
            const response = await request(app).post('/mint/biobot').send(validMintPayload);
            expect(response.statusCode).toBe(500);
            expect(response.body.error).toContain('Failed to pin image to IPFS.');
            expect(response.body.details).toContain('Simulated Pinata API Network Error');
        });

        it('should return 500 if metadata generation/pinning (generateAndPinMetadata) fails', async () => {
            axios.post.mockResolvedValueOnce({ data: { IpfsHash: 'QmAnyImageHash' } }); // Image pin success
            generateAndPinMetadata.mockRejectedValueOnce(new Error('Simulated Metadata Pinning Error')); // Metadata fail
            const response = await request(app).post('/mint/biobot').send(validMintPayload);
            expect(response.statusCode).toBe(500);
            expect(response.body.error).toContain('Failed to generate or pin metadata.');
            expect(response.body.details).toContain('Simulated Metadata Pinning Error');
        });

        it('should return 500 if smart contract mintBiobot transaction reverts (status 0)', async () => {
            axios.post.mockResolvedValueOnce({ data: { IpfsHash: 'QmAnyImageHash' } });
            generateAndPinMetadata.mockResolvedValueOnce('ipfs://QmAnyMetadataHash');
            mockMintBiobot.mockResolvedValueOnce({
                hash: '0xTxHashForRevertedTx',
                wait: jest.fn().mockResolvedValueOnce({ status: 0, transactionHash: '0xTxHashForRevertedTx' }) // status 0 for failure
            });
            const response = await request(app).post('/mint/biobot').send(validMintPayload);
            expect(response.statusCode).toBe(500);
            expect(response.body.error).toContain('Smart contract transaction failed for Biobot.');
        });

        it('should return 500 if smart contract mintBiobot call itself throws an error (e.g., RPC issue)', async () => {
            axios.post.mockResolvedValueOnce({ data: { IpfsHash: 'QmAnyImageHash' } });
            generateAndPinMetadata.mockResolvedValueOnce('ipfs://QmAnyMetadataHash');
            mockMintBiobot.mockRejectedValueOnce(new Error('Simulated Blockchain RPC Communication Error'));
            const response = await request(app).post('/mint/biobot').send(validMintPayload);
            expect(response.statusCode).toBe(500);
            expect(response.body.error).toContain('Error interacting with smart contract for Biobot.');
            expect(response.body.details).toContain('Simulated Blockchain RPC Communication Error');
        });
    });

    // --- Placeholder for /mint/gravastar tests ---
    describe('POST /mint/gravastar', () => {
        it.todo('should successfully mint a Gravastar NFT with valid data and mocks');
        // Add other Gravastar specific minting tests:
        // - Missing gravastarData
        // - Errors during image pinning (if Gravastars also have images from serverImageFileName)
        // - Errors during metadata for Gravastar
        // - Errors during GravastarNFT contract call
    });
});
