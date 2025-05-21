// Dalax_BiobotProject/BlockchainIntegration/deploy/00-deploy-biobot-nft.js
// This script deploys the BiobotNFT.sol contract.

async function main() {
    // Get the deployer (the account that will deploy the contract)
    const [deployer] = await ethers.getSigners();

    console.log("Deploying BiobotNFT contract with the account:", deployer.address);
    console.log("Account balance:", (await deployer.getBalance()).toString());

    // Get the ContractFactory for BiobotNFT
    // Ensure your contract name in the getContractFactory call matches your .sol file name
    const BiobotNFTFactory = await ethers.getContractFactory("BiobotNFT");

    // Deploy the contract
    // The constructor for BiobotNFT is: constructor() ERC721("Dalax Biobot", "DBOT") {}
    // It does not take any arguments, so we don't pass any to deploy().
    const biobotNFT = await BiobotNFTFactory.deploy();

    // Wait for the deployment to be confirmed
    await biobotNFT.deployed();

    console.log("BiobotNFT deployed to:", biobotNFT.address);

    // Optional: Save the contract address and ABI to a file for frontend/backend use
    saveFrontendFiles(biobotNFT, "BiobotNFT");
}

function saveFrontendFiles(contract, contractName) {
    const fs = require("fs");
    // Adjust this path if your metadata/contracts directory is elsewhere relative to this script
    const contractsDir = __dirname + "/../../metadata/contractsInfo"; 

    if (!fs.existsSync(contractsDir)) {
        fs.mkdirSync(contractsDir, { recursive: true });
    }

    fs.writeFileSync(
        contractsDir + `/${contractName}-address.json`,
        JSON.stringify({ address: contract.address }, undefined, 2)
    );

    // Make sure to use the correct artifact name if it differs from the contract name
    const contractArtifact = artifacts.readArtifactSync(contractName);

    fs.writeFileSync(
        contractsDir + `/${contractName}-abi.json`, // Saves the ABI
        JSON.stringify(contractArtifact.abi, null, 2) // Only save the ABI part
    );

    console.log(`Saved ${contractName} address and ABI to ${contractsDir}`);
    console.log(`Note: You might want to copy these to your UnityClient's Resources or a config file for your mint_api.js`);
}

main()
    .then(() => process.exit(0))
    .catch((error) => {
        console.error(error);
        process.exit(1);
    });
