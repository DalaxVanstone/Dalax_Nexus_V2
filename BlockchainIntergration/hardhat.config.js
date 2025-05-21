require('@nomiclabs/hardhat-waffle');

module.exports = {
  solidity: '0.8.4',
  networks: {
    localhost: {
      url: 'http://127.0.0.1:8545'
    },
    polygon: {
      url: 'https://polygon-rpc.com',
      accounts: ['<PRIVATE_KEY>']
    }
  }
};