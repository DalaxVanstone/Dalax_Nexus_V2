// This script deploys the GravastarNFT.sol contract.

async function main() {
    // Get the deployer (the account that will deploy the contract)
    const [deployer] = await ethers.getSigners();

    console.log("Deploying GravastarNFT contract with the account:", deployer.address);
    console.log("Account balance:", (await deployer.getBalance()).toString());

    // Get the ContractFactory for GravastarNFT
    const GravastarNFTFactory = await ethers.getContractFactory("GravastarNFT");

    // Deploy the contract
    // The constructor for GravastarNFT is: constructor() ERC721("GravastarCore", "GSC") {}
    // It does not take any arguments, so we don't pass any to deploy().
    const gravastarNFT = await GravastarNFTFactory.deploy();

    // Wait for the deployment to be confirmed
    await gravastarNFT.deployed();

    console.log("GravastarNFT deployed to:", gravastarNFT.address);

    // Optional: Save the contract address and ABI to a file for frontend use
    // This is a common practice. You might want to adapt this part to your project structure.
    saveFrontendFiles(gravastarNFT, "GravastarNFT");
}

function saveFrontendFiles(contract, contractName) {
    const fs = require("fs");
    const contractsDir = __dirname + "/../../metadata/contracts"; // Example: Save in metadata/contracts

    if (!fs.existsSync(contractsDir)) {
        fs.mkdirSync(contractsDir, { recursive: true });
    }

    fs.writeFileSync(
        contractsDir + `/${contractName}-address.json`,
        JSON.stringify({ address: contract.address }, undefined, 2)
    );

    const contractArtifact = artifacts.readArtifactSync(contractName);

    fs.writeFileSync(
        contractsDir + `/${contractName}.json`, // Saves the ABI
        JSON.stringify(contractArtifact, null, 2)
    );

    console.log(`Saved ${contractName} address and ABI to ${contractsDir}`);
}

main()
    .then(() => process.exit(0))
    .catch((error) => {
        console.error(error);
        process.exit(1);
    });
