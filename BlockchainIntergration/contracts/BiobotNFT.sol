// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/Counters.sol"; // Helpful for token IDs

contract BiobotNFT is ERC721URIStorage, Ownable {
    using Counters for Counters.Counter;
    Counters.Counter private _tokenIds; // Private counter for token IDs

    // Event to announce a new Biobot minting
    event BiobotMinted(uint256 indexed tokenId, address indexed owner, string tokenURI);

    constructor() ERC721("Dalax Biobot", "DBOT") {
        // ERC721 constructor takes Name and Symbol
    }

    /**
     * @dev Mints a new Biobot NFT.
     * Only the owner (deployer or an authorized address) can mint.
     * @param recipient The address that will receive the minted NFT.
     * @param tokenURI_ The URI string that points to the NFT's metadata.
     * @return The ID of the newly minted token.
     */
    function mintBiobot(address recipient, string memory tokenURI_) public onlyOwner returns (uint256) {
        _tokenIds.increment(); // Increment counter to get new ID
        uint256 newItemId = _tokenIds.current(); // Get the new ID

        _mint(recipient, newItemId); // Mint the NFT to the recipient with the new ID
        _setTokenURI(newItemId, tokenURI_); // Set the metadata URI for the new NFT

        emit BiobotMinted(newItemId, recipient, tokenURI_); // Emit an event

        return newItemId; // Return the new token ID
    }

    /**
     * @dev Returns the total number of tokens minted so far.
     */
    function totalSupply() public view returns (uint256) {
        return _tokenIds.current();
    }
}
