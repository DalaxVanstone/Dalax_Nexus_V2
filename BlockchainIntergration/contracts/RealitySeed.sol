// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract RealitySeed is ERC721URIStorage, Ownable {
    uint256 public nextSeed;

    constructor() ERC721("RealitySeed", "SEED") {}

    function createSeed(string memory metadataUri) external onlyOwner returns (uint256) {
        uint256 id = nextSeed++;
        _safeMint(msg.sender, id);
        _setTokenURI(id, metadataUri);
        return id;
    }
}