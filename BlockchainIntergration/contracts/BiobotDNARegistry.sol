// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

contract BiobotDNARegistry {
    struct BiobotDNA {
        string sequence;
        uint timestamp;
    }

    mapping(address => BiobotDNA[]) public registry;

    function registerDNA(string memory sequence) public {
        registry[msg.sender].push(BiobotDNA(sequence, block.timestamp));
    }

    function getDNACount(address user) public view returns (uint) {
        return registry[user].length;
    }
}
