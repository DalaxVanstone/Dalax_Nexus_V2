// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "./DLXC.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract YieldHarvester is Ownable {
    DLXC public dlxc;
    uint256 public rewardRate; // DLXC per score point

    constructor(address dlxcAddress, uint256 _rewardRate) {
        dlxc = DLXC(dlxcAddress);
        rewardRate = _rewardRate;
    }

    function harvest(address to, uint256 score) external onlyOwner {
        uint256 amount = score * rewardRate;
        dlxc.mint(to, amount);
    }

    function setRate(uint256 _rate) external onlyOwner {
        rewardRate = _rate;
    }
}