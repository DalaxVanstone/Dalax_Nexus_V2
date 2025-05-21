const { expect } = require("chai");
const { ethers } = require("hardhat"); // Hardhat injects ethers here

describe("BiobotNFT Contract", function () {
    let BiobotNFT;
    let biobotNFT;
    let owner;
    let addr1;
    let addr2;
    let addrs;

    const contractURI = "ipfs://YOUR_CONTRACT_LEVEL_METADATA_HASH_BIOBOT"; // Replace if you set one
    const baseTokenName = "Dalax Biobot"; // Assuming your contract sets a name like "Dalax Biobot"
    const baseTokenSymbol = "DBB";     // Assuming your contract sets a symbol like "DBB"

    // Runs before each test in this describe block
    beforeEach(async function () {
        // Get the ContractFactory and Signers here.
        BiobotNFT = await ethers.getContractFactory("BiobotNFT");
        [owner, addr1, addr2, ...addrs] = await ethers.getSigners();

        // Deploy a new instance of the contract before each test
        biobotNFT = await BiobotNFT.deploy(contractURI);
        // await biobotNFT.deployed(); // Not strictly needed with Hardhat's ethers, deploy() waits.
    });

    describe("Deployment", function () {
        it("Should set the correct owner", async function () {
            expect(await biobotNFT.owner()).to.equal(owner.address);
        });

        it("Should have the correct name and symbol", async function () {
            expect(await biobotNFT.name()).to.equal(baseTokenName);
            expect(await biobotNFT.symbol()).to.equal(baseTokenSymbol);
        });

        it("Should set the contract URI correctly", async function () {
            // This test assumes your contract has a contractURI() function, common with ERC721C.
            // If not, or if it's set differently (e.g., in a separate metadata extension), adjust this.
            // For a simple ERC721, there might not be a contract-level URI by default.
            if (typeof biobotNFT.contractURI === "function") {
                 expect(await biobotNFT.contractURI()).to.equal(contractURI);
            } else {
                console.warn("Skipping contractURI test: function not found on contract.");
            }
        });
    });

    describe("Minting Biobots (mintBiobot)", function () {
        const tokenId1 = ethers.BigNumber.from(0); // Or 1 if your contract starts token IDs from 1
        const tokenURI1 = "ipfs://METADATA_HASH_FOR_BIOBOT_1";

        it("Owner should be able to mint a new Biobot NFT", async function () {
            await expect(biobotNFT.connect(owner).mintBiobot(addr1.address, tokenId1, tokenURI1))
                .to.emit(biobotNFT, "BiobotMinted") // Assuming you have this custom event
                .withArgs(addr1.address, tokenId1, tokenURI1)
                .and.to.emit(biobotNFT, "Transfer") // Standard ERC721 Transfer event
                .withArgs(ethers.constants.AddressZero, addr1.address, tokenId1);

            expect(await biobotNFT.ownerOf(tokenId1)).to.equal(addr1.address);
            expect(await biobotNFT.tokenURI(tokenId1)).to.equal(tokenURI1);
            expect(await biobotNFT.balanceOf(addr1.address)).to.equal(1);
        });

        it("Should assign token IDs incrementally if _tokenIdCounter is used", async function() {
            // This test assumes your contract uses an internal counter for token IDs.
            // If your mintBiobot explicitly takes a tokenId, this test needs adjustment or refers to that.
            // Let's assume for this test mintBiobot generates IDs.
            // If mintBiobot takes an explicit tokenId (as in our current mock for it), we test that ID is used.

            const tokenURI2 = "ipfs://METADATA_HASH_FOR_BIOBOT_2";
            const tokenId2 = ethers.BigNumber.from(1); // Assuming previous test used 0, or adjust based on counter.

            // If your contract's mintBiobot *doesn't* take tokenId as a parameter, but generates it:
            // await biobotNFT.connect(owner).mintBiobot(addr1.address, tokenURI1); // Mints token 0 (or 1)
            // await biobotNFT.connect(owner).mintBiobot(addr2.address, tokenURI2); // Mints token 1 (or 2)
            // expect(await biobotNFT.ownerOf(tokenIdCounterInitialValue.add(1))).to.equal(addr2.address);

            // If it *does* take tokenId:
            await biobotNFT.connect(owner).mintBiobot(addr1.address, tokenId1, tokenURI1);
            await biobotNFT.connect(owner).mintBiobot(addr2.address, tokenId2, tokenURI2);
            expect(await biobotNFT.ownerOf(tokenId2)).to.equal(addr2.address);
        });

        it("Should fail if a non-owner tries to mint", async function () {
            await expect(
                biobotNFT.connect(addr1).mintBiobot(addr1.address, tokenId1, tokenURI1)
            ).to.be.revertedWithCustomError(biobotNFT, "OwnableUnauthorizedAccount") // OpenZeppelin 5.x Ownable error
             .withArgs(addr1.address); 
        });

        it("Should fail to mint to the zero address", async function () {
            // ERC721 standard often prevents minting to address(0)
            await expect(
                biobotNFT.connect(owner).mintBiobot(ethers.constants.AddressZero, tokenId1, tokenURI1)
            ).to.be.revertedWithCustomError(biobotNFT, "ERC721InvalidReceiver") // OpenZeppelin 5.x ERC721 error
             .withArgs(ethers.constants.AddressZero);
        });

        it("Should fail if trying to mint a token ID that already exists", async function () {
            await biobotNFT.connect(owner).mintBiobot(addr1.address, tokenId1, tokenURI1);
            await expect(
                biobotNFT.connect(owner).mintBiobot(addr2.address, tokenId1, "ipfs://DIFFERENT_URI")
            ).to.be.revertedWithCustomError(biobotNFT, "ERC721ExistingToken") // OpenZeppelin 5.x ERC721 error
             .withArgs(tokenId1);
        });

        it("Should correctly set token URI during mint", async function () {
            await biobotNFT.connect(owner).mintBiobot(addr1.address, tokenId1, tokenURI1);
            expect(await biobotNFT.tokenURI(tokenId1)).to.equal(tokenURI1);
        });
    });

    describe("Token URI Management", function () {
        const tokenId1 = ethers.BigNumber.from(0);
        const tokenURI1 = "ipfs://InitialMetadata/";
        const newTokenURI = "ipfs://UpdatedMetadata/";

        beforeEach(async function() {
            await biobotNFT.connect(owner).mintBiobot(addr1.address, tokenId1, tokenURI1);
        });

        it("Should return the correct token URI for an existing token", async function () {
            expect(await biobotNFT.tokenURI(tokenId1)).to.equal(tokenURI1);
        });

        it("Should revert when querying URI for a non-existent token", async function () {
            const nonExistentTokenId = ethers.BigNumber.from(999);
            await expect(biobotNFT.tokenURI(nonExistentTokenId))
                .to.be.revertedWithCustomError(biobotNFT, "ERC721NonexistentToken") // OpenZeppelin 5.x ERC721 error
                .withArgs(nonExistentTokenId);
        });

        // If your contract allows updating token URIs (e.g., via _setTokenURI and an owner-only function)
        // Add tests for that functionality here. For a basic ERC721 + Ownable mint, URI is often immutable post-mint by default.
        // Example if you added a `setTokenURI` function:
        /*
        it("Owner should be able to update token URI if setTokenURI function exists", async function () {
            if (typeof biobotNFT.setTokenURI === "function") {
                await expect(biobotNFT.connect(owner).setTokenURI(tokenId1, newTokenURI))
                    .to.emit(biobotNFT, "MetadataUpdate") // Or appropriate event
                    .withArgs(tokenId1);
                expect(await biobotNFT.tokenURI(tokenId1)).to.equal(newTokenURI);
            } else {
                this.skip(); // Skip if function doesn't exist
            }
        });

        it("Non-owner should not be able to update token URI", async function () {
            if (typeof biobotNFT.setTokenURI === "function") {
                await expect(
                    biobotNFT.connect(addr1).setTokenURI(tokenId1, newTokenURI)
                ).to.be.revertedWith("Ownable: caller is not the owner"); // Or OwnableUnauthorizedAccount
            } else {
                this.skip();
            }
        });
        */
    });

    describe("ERC721 Standard Functionality (Ownership & Transfers)", function () {
        const tokenId1 = ethers.BigNumber.from(0);
        const tokenId2 = ethers.BigNumber.from(1);
        const tokenURI1 = "ipfs://metadata1";
        const tokenURI2 = "ipfs://metadata2";

        beforeEach(async function() {
            // Mint two tokens: tokenId1 to addr1, tokenId2 to addr2
            await biobotNFT.connect(owner).mintBiobot(addr1.address, tokenId1, tokenURI1);
            await biobotNFT.connect(owner).mintBiobot(addr2.address, tokenId2, tokenURI2);
        });

        it("Should correctly report ownerOf", async function() {
            expect(await biobotNFT.ownerOf(tokenId1)).to.equal(addr1.address);
            expect(await biobotNFT.ownerOf(tokenId2)).to.equal(addr2.address);
        });

        it("Should correctly report balanceOf", async function() {
            expect(await biobotNFT.balanceOf(addr1.address)).to.equal(1);
            expect(await biobotNFT.balanceOf(addr2.address)).to.equal(1);
            expect(await biobotNFT.balanceOf(owner.address)).to.equal(0);
        });

        it("Should allow owner to transfer their token", async function() {
            await biobotNFT.connect(addr1).transferFrom(addr1.address, addr2.address, tokenId1);
            expect(await biobotNFT.ownerOf(tokenId1)).to.equal(addr2.address);
            expect(await biobotNFT.balanceOf(addr1.address)).to.equal(0);
            expect(await biobotNFT.balanceOf(addr2.address)).to.equal(2); // addr2 now has tokenId1 and tokenId2
        });

        it("Should allow approved address to transfer token", async function() {
            await biobotNFT.connect(addr1).approve(owner.address, tokenId1); // addr1 approves owner for tokenId1
            expect(await biobotNFT.getApproved(tokenId1)).to.equal(owner.address);
            await biobotNFT.connect(owner).transferFrom(addr1.address, addr2.address, tokenId1); // owner transfers on behalf of addr1
            expect(await biobotNFT.ownerOf(tokenId1)).to.equal(addr2.address);
        });

        it("Should allow operator (approved for all) to transfer token", async function() {
            await biobotNFT.connect(addr1).setApprovalForAll(owner.address, true); // addr1 approves owner for all its tokens
            expect(await biobotNFT.isApprovedForAll(addr1.address, owner.address)).to.be.true;
            await biobotNFT.connect(owner).transferFrom(addr1.address, addr2.address, tokenId1); // owner transfers tokenId1
            expect(await biobotNFT.ownerOf(tokenId1)).to.equal(addr2.address);
            // Operator approval should persist for other tokens if any
        });

        it("Should fail if non-approved, non-owner tries to transfer", async function() {
            await expect(
                biobotNFT.connect(addrs[0]).transferFrom(addr1.address, addr2.address, tokenId1)
            ).to.be.revertedWithCustomError(biobotNFT, "ERC721InsufficientApproval") // OpenZeppelin 5.x ERC721 error
             .withArgs(addrs[0].address, tokenId1);
        });
        
        it("Using safeTransferFrom successfully", async function() {
             await biobotNFT.connect(addr1)["safeTransferFrom(address,address,uint256)"](addr1.address, addr2.address, tokenId1);
             expect(await biobotNFT.ownerOf(tokenId1)).to.equal(addr2.address);
        });
    });

    // Add tests for ERC721Burnable if your contract inherits it
    describe("ERC721Burnable Functionality (if applicable)", function() {
        const tokenIdToBurn = ethers.BigNumber.from(0);
        const tokenURI = "ipfs://burnableTokenURI";

        beforeEach(async function() {
            await biobotNFT.connect(owner).mintBiobot(addr1.address, tokenIdToBurn, tokenURI);
        });

        it("Owner of token should be able to burn it", async function () {
            // Check if burn function exists (common for ERC721Burnable)
            if (typeof biobotNFT.connect(addr1).burn !== "function") {
                this.skip(); // Skip if burn is not a function
            }
            await expect(biobotNFT.connect(addr1).burn(tokenIdToBurn))
                .to.emit(biobotNFT, "Transfer")
                .withArgs(addr1.address, ethers.constants.AddressZero, tokenIdToBurn);
            
            expect(await biobotNFT.balanceOf(addr1.address)).to.equal(0);
            await expect(biobotNFT.ownerOf(tokenIdToBurn))
                .to.be.revertedWithCustomError(biobotNFT, "ERC721NonexistentToken")
                .withArgs(tokenIdToBurn);
        });

        it("Approved address should be able to burn token", async function() {
            if (typeof biobotNFT.connect(owner).burn !== "function") { this.skip(); }
            
            await biobotNFT.connect(addr1).approve(owner.address, tokenIdToBurn);
            await expect(biobotNFT.connect(owner).burn(tokenIdToBurn))
                .to.emit(biobotNFT, "Transfer")
                .withArgs(addr1.address, ethers.constants.AddressZero, tokenIdToBurn);
            expect(await biobotNFT.balanceOf(addr1.address)).to.equal(0);
        });

        it("Should fail if non-owner and non-approved tries to burn", async function() {
            if (typeof biobotNFT.connect(addr2).burn !== "function") { this.skip(); }

            // This error depends on how burn checks permissions.
            // OpenZeppelin's ERC721Burnable checks _isApprovedOrOwner.
            // If it directly checks msg.sender == ownerOf(tokenId), the error might be different
            // For OZ's _burn, it doesn't have a specific "ERC721InsufficientApprovalForBurn".
            // It might revert due to ownerOf check or if _isApprovedOrOwner fails.
            // Often, it might be "ERC721BurnForbidden" or similar if checks are explicit,
            // or it could be a generic revert if an internal check fails.
            // Let's assume it implies an approval/ownership check.
            await expect(
                biobotNFT.connect(addr2).burn(tokenIdToBurn)
            ).to.be.reverted; // OZ's burn reverts if not owner or approved without specific error string.
                              // Or use .to.be.revertedWithCustomError(biobotNFT, "ERC721Forbidden"); or similar if your OZ version has it for burn.
        });
    });
    
    // Add tests for Pausable functionality if your contract inherits Pausable
    describe("Pausable Functionality (if applicable)", function() {
        const tokenId = ethers.BigNumber.from(0);
        const tokenURI = "ipfs://pausableTokenURI";

        beforeEach(async function() {
            await biobotNFT.connect(owner).mintBiobot(addr1.address, tokenId, tokenURI);
        });

        it("Owner should be able to pause and unpause minting/transfers", async function() {
            if (typeof biobotNFT.pause !== "function") { this.skip(); }

            await expect(biobotNFT.connect(owner).pause())
                .to.emit(biobotNFT, "Paused")
                .withArgs(owner.address);
            expect(await biobotNFT.paused()).to.equal(true);

            // Test that minting is blocked when paused
            await expect(
                biobotNFT.connect(owner).mintBiobot(addr2.address, ethers.BigNumber.from(1), "ipfs://anotherURI")
            ).to.be.revertedWithCustomError(biobotNFT, "EnforcedPause"); // OpenZeppelin 5.x Pausable error

            // Test that transfers are blocked when paused
            await expect(
                biobotNFT.connect(addr1).transferFrom(addr1.address, addr2.address, tokenId)
            ).to.be.revertedWithCustomError(biobotNFT, "EnforcedPause");

            await expect(biobotNFT.connect(owner).unpause())
                .to.emit(biobotNFT, "Unpaused")
                .withArgs(owner.address);
            expect(await biobotNFT.paused()).to.equal(false);

            // Minting should work again
            await expect(biobotNFT.connect(owner).mintBiobot(addr2.address, ethers.BigNumber.from(1), "ipfs://anotherURI")).to.not.be.reverted;
        });

        it("Non-owner should not be able to pause or unpause", async function() {
            if (typeof biobotNFT.pause !== "function") { this.skip(); }
            await expect(biobotNFT.connect(addr1).pause())
                .to.be.revertedWithCustomError(biobotNFT, "OwnableUnauthorizedAccount")
                .withArgs(addr1.address);
            
            // Pause as owner first to test unpause by non-owner
            await biobotNFT.connect(owner).pause();
            await expect(biobotNFT.connect(addr1).unpause())
                .to.be.revertedWithCustomError(biobotNFT, "OwnableUnauthorizedAccount")
                .withArgs(addr1.address);
        });
    });
});
