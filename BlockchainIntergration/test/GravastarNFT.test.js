const { expect } = require("chai");
const { ethers } = require("hardhat"); // Hardhat injects ethers here

describe("GravastarNFT Contract", function () {
    let GravastarNFTFactory;
    let gravastarNFT;
    let owner;
    let addr1;
    let addr2;
    let addrs;

    // Define default values that should match your GravastarNFT contract's constructor or initialization
    const DEFAULT_CONTRACT_URI_GRAVASTAR = "ipfs://YOUR_GRAVASTAR_CONTRACT_METADATA_HASH_HERE"; // Replace
    const TOKEN_NAME_GRAVASTAR = "Dalax Gravastar"; // Expected name from your contract
    const TOKEN_SYMBOL_GRAVASTAR = "DGS";      // Expected symbol from your contract

    beforeEach(async function () {
        GravastarNFTFactory = await ethers.getContractFactory("GravastarNFT");
        [owner, addr1, addr2, ...addrs] = await ethers.getSigners();

        // Deploy a new instance of the contract
        // Pass constructor arguments if your GravastarNFT.sol has them
        gravastarNFT = await GravastarNFTFactory.deploy(DEFAULT_CONTRACT_URI_GRAVASTAR);
    });

    // --- Section: Deployment ---
    describe("Deployment", function () {
        it("Should set the correct owner", async function () {
            expect(await gravastarNFT.owner()).to.equal(owner.address);
        });

        it("Should have the correct name and symbol", async function () {
            expect(await gravastarNFT.name()).to.equal(TOKEN_NAME_GRAVASTAR);
            expect(await gravastarNFT.symbol()).to.equal(TOKEN_SYMBOL_GRAVASTAR);
        });

        it("Should set the contract URI correctly upon deployment", async function () {
            if (typeof gravastarNFT.contractURI === "function") {
                expect(await gravastarNFT.contractURI()).to.equal(DEFAULT_CONTRACT_URI_GRAVASTAR);
            } else {
                console.warn("      Skipping contractURI() test for GravastarNFT: function not found.");
                this.skip();
            }
        });
    });

    // --- Section: Minting Gravastars (e.g., mintGravastar function) ---
    describe("Minting Gravastars", function () {
        const tokenId1 = ethers.BigNumber.from(0); 
        const tokenURI1 = "ipfs://METADATA_FOR_GRAVASTAR_0";
        const tokenId2 = ethers.BigNumber.from(1);
        const tokenURI2 = "ipfs://METADATA_FOR_GRAVASTAR_1";

        // Assuming your GravastarNFT.sol has a minting function like:
        // function mintGravastar(address recipient, uint256 tokenId, string memory tokenURI) public onlyOwner { ... }
        // And an event like: event GravastarMinted(address indexed recipient, uint256 indexed tokenId, string tokenURI);

        it("Owner should be able to mint a new Gravastar NFT", async function () {
            await expect(gravastarNFT.connect(owner).mintGravastar(addr1.address, tokenId1, tokenURI1))
                .to.emit(gravastarNFT, "GravastarMinted") // Your custom event for Gravastar
                .withArgs(addr1.address, tokenId1, tokenURI1)
                .and.to.emit(gravastarNFT, "Transfer")   // Standard ERC721 Transfer event
                .withArgs(ethers.constants.AddressZero, addr1.address, tokenId1);

            expect(await gravastarNFT.ownerOf(tokenId1)).to.equal(addr1.address);
            expect(await gravastarNFT.tokenURI(tokenId1)).to.equal(tokenURI1);
            expect(await gravastarNFT.balanceOf(addr1.address)).to.equal(1);
        });

        it("Should correctly assign explicitly provided token IDs during mint", async function() {
            await gravastarNFT.connect(owner).mintGravastar(addr1.address, tokenId1, tokenURI1);
            await gravastarNFT.connect(owner).mintGravastar(addr2.address, tokenId2, tokenURI2);
            
            expect(await gravastarNFT.ownerOf(tokenId1)).to.equal(addr1.address);
            expect(await gravastarNFT.ownerOf(tokenId2)).to.equal(addr2.address);
        });

        it("Should fail if a non-owner tries to mint", async function () {
            await expect(
                gravastarNFT.connect(addr1).mintGravastar(addr1.address, tokenId1, tokenURI1)
            ).to.be.revertedWithCustomError(gravastarNFT, "OwnableUnauthorizedAccount")
             .withArgs(addr1.address);
        });

        it("Should fail to mint to the zero address", async function () {
            await expect(
                gravastarNFT.connect(owner).mintGravastar(ethers.constants.AddressZero, tokenId1, tokenURI1)
            ).to.be.revertedWithCustomError(gravastarNFT, "ERC721InvalidReceiver")
             .withArgs(ethers.constants.AddressZero);
        });

        it("Should fail if trying to mint a token ID that already exists", async function () {
            await gravastarNFT.connect(owner).mintGravastar(addr1.address, tokenId1, tokenURI1);
            await expect(
                gravastarNFT.connect(owner).mintGravastar(addr2.address, tokenId1, "ipfs://DIFFERENT_URI_FOR_EXISTING_GRAVASTAR_ID")
            ).to.be.revertedWithCustomError(gravastarNFT, "ERC721ExistingToken")
             .withArgs(tokenId1);
        });
    });

    // --- Section: Token URI Management (Assuming similar to BiobotNFT) ---
    describe("Token URI Management for GravastarNFT", function () {
        const tokenId = ethers.BigNumber.from(50);
        const initialURI = "ipfs://GravastarInitialMetadata/50";
        const updatedURI = "ipfs://GravastarUpdatedMetadata/50";

        beforeEach(async function() {
            await gravastarNFT.connect(owner).mintGravastar(addr1.address, tokenId, initialURI);
        });

        it("Should return the correct token URI for an existing Gravastar token", async function () {
            expect(await gravastarNFT.tokenURI(tokenId)).to.equal(initialURI);
        });

        it("Should revert when querying URI for a non-existent Gravastar token", async function () {
            const nonExistentTokenId = ethers.BigNumber.from(9999);
            await expect(gravastarNFT.tokenURI(nonExistentTokenId))
                .to.be.revertedWithCustomError(gravastarNFT, "ERC721NonexistentToken")
                .withArgs(nonExistentTokenId);
        });
        
        // Conditional test for updating token URI if GravastarNFT supports it
        it("Owner should be able to update token URI if a 'setTokenURI' function exists for Gravastars", async function () {
            if (typeof gravastarNFT.setTokenURI !== "function") {
                console.warn("      Skipping setTokenURI test for GravastarNFT: function not found.");
                this.skip();
            }
            // Assuming an event like "MetadataUpdate" or "TokenURIUpdate"
            await expect(gravastarNFT.connect(owner).setTokenURI(tokenId, updatedURI))
                .to.emit(gravastarNFT, "MetadataUpdate") 
                .withArgs(tokenId);
            expect(await gravastarNFT.tokenURI(tokenId)).to.equal(updatedURI);
        });
    });

    // --- Section: Standard ERC721 Functionality (Should be identical to BiobotNFT if both are standard ERC721) ---
    describe("ERC721 Standard Functionality for GravastarNFT", function () {
        const tid1 = ethers.BigNumber.from(100);
        const tid2 = ethers.BigNumber.from(101);
        const uri1 = "ipfs://gravastar_erc721_metadata1";
        const uri2 = "ipfs://gravastar_erc721_metadata2";

        beforeEach(async function() {
            await gravastarNFT.connect(owner).mintGravastar(addr1.address, tid1, uri1);
            await gravastarNFT.connect(owner).mintGravastar(addr2.address, tid2, uri2);
        });

        it("Should correctly report ownerOf for Gravastar tokens", async function() {
            expect(await gravastarNFT.ownerOf(tid1)).to.equal(addr1.address);
            expect(await gravastarNFT.ownerOf(tid2)).to.equal(addr2.address);
        });

        it("Should correctly report balanceOf for Gravastar tokens", async function() {
            expect(await gravastarNFT.balanceOf(addr1.address)).to.equal(1);
            expect(await gravastarNFT.balanceOf(addr2.address)).to.equal(1);
        });

        it("Gravastar token owner should be able to transfer their token using transferFrom", async function() {
            await expect(gravastarNFT.connect(addr1).transferFrom(addr1.address, addr2.address, tid1))
                .to.emit(gravastarNFT, "Transfer").withArgs(addr1.address, addr2.address, tid1);
            expect(await gravastarNFT.ownerOf(tid1)).to.equal(addr2.address);
        });
        // Add more transfer, approval, safeTransferFrom tests similar to BiobotNFT.test.js
    });

    // --- Section: ERC721Burnable Functionality for GravastarNFT (Conditional) ---
    describe("ERC721Burnable Functionality for GravastarNFT (if applicable)", function() {
        const burnableTokenId = ethers.BigNumber.from(77);
        const burnableURI = "ipfs://burnThisGravastar";

        beforeEach(async function() {
            if (typeof gravastarNFT.burn !== "function") {
                this.skip(); // Skip all tests in this describe block
            }
            await gravastarNFT.connect(owner).mintGravastar(addr1.address, burnableTokenId, burnableURI);
        });

        it("Owner of Gravastar token should be able to burn it", async function () {
            await expect(gravastarNFT.connect(addr1).burn(burnableTokenId))
                .to.emit(gravastarNFT, "Transfer")
                .withArgs(addr1.address, ethers.constants.AddressZero, burnableTokenId);
            await expect(gravastarNFT.ownerOf(burnableTokenId))
                .to.be.revertedWithCustomError(gravastarNFT, "ERC721NonexistentToken");
        });
        // Add more burn tests (approved, non-owner fail) similar to BiobotNFT.test.js
    });
    
    // --- Section: Pausable Functionality for GravastarNFT (Conditional) ---
    describe("Pausable Functionality for GravastarNFT (if applicable)", function() {
        const pausableTokenId = ethers.BigNumber.from(88);
        const pausableURI = "ipfs://pausableGravastar";

        beforeEach(async function() {
            if (typeof gravastarNFT.pause !== "function") {
                this.skip(); // Skip all tests in this describe block
            }
            await gravastarNFT.connect(owner).mintGravastar(addr1.address, pausableTokenId, pausableURI);
        });

        it("Owner (pauser) should be able to pause and unpause GravastarNFT contract", async function() {
            await expect(gravastarNFT.connect(owner).pause())
                .to.emit(gravastarNFT, "Paused").withArgs(owner.address);
            expect(await gravastarNFT.paused()).to.equal(true);

            await expect(gravastarNFT.connect(owner).unpause())
                .to.emit(gravastarNFT, "Unpaused").withArgs(owner.address);
            expect(await gravastarNFT.paused()).to.equal(false);
        });

        it("Gravastar minting and transfers should be blocked when paused", async function() {
            await gravastarNFT.connect(owner).pause();
            const newTokenId = ethers.BigNumber.from(89);
            await expect(
                gravastarNFT.connect(owner).mintGravastar(addr2.address, newTokenId, "ipfs://pausedMintURI")
            ).to.be.revertedWithCustomError(gravastarNFT, "EnforcedPause");
            await expect(
                gravastarNFT.connect(addr1).transferFrom(addr1.address, addr2.address, pausableTokenId)
            ).to.be.revertedWithCustomError(gravastarNFT, "EnforcedPause");
            
            await gravastarNFT.connect(owner).unpause(); // Cleanup for other tests
        });
        // Add non-owner pause/unpause failure tests similar to BiobotNFT.test.js
    });
});
