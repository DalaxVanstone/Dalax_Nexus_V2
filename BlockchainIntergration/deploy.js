
const hre = require("hardhat");

async function main() {
    const BiobotNFT = await hre.ethers.getContractFactory("BiobotNFT");
    const nft = await BiobotNFT.deploy();
    await nft.deployed();
    console.log("BiobotNFT deployed at:", nft.address);
}

main().catch((error) => {
    console.error(error);
    process.exitCode = 1;
});
