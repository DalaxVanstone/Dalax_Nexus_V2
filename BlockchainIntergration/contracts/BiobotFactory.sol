// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "@openzeppelin/contracts/token/ERC721/extensions/ERC721Enumerable.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract BiobotFactory is ERC721Enumerable, Ownable {
    uint256 public nextId;
    mapping(uint256 => string) public tokenURI;

    constructor() ERC721("Biobot", "BOT") {}

    function mintBiobot(address to, string memory uri) external onlyOwner returns (uint256) {
        uint256 id = nextId++;
        _safeMint(to, id);
        tokenURI[id] = uri;
        return id;
    }

    function _baseURI() internal view override returns (string memory) {
        return "";
    }

    function tokenURI(uint256 tokenId) public view override returns (string memory) {
        return tokenURI[tokenId];
    }
}