const express = require('express');
const { ethers } = require('ethers');
const fs = require('fs');
const app = express();
app.use(express.json());

const abi = JSON.parse(fs.readFileSync('../contracts/BiobotDNARegistry.json')).abi;
const contractAddress = "0x123456789..."; // Replace with actual contract address
const provider = new ethers.providers.JsonRpcProvider("http://localhost:8545");
const signer = provider.getSigner();

const registry = new ethers.Contract(contractAddress, abi, signer);

app.post("/register", async (req, res) => {
    const { sequence } = req.body;
    const tx = await registry.registerDNA(sequence);
    await tx.wait();
    res.send("DNA Registered");
});

app.listen(8545, () => console.log("Blockchain middleware running on port 8545"));
