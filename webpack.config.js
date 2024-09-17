const path = require('path');

module.exports = {
    entry: './wwwroot/javascript/index.js',
    output: {
        filename: 'bundle.js',
        path: path.resolve(__dirname, 'wwwroot/dist'),
        library: 'SuperInvestor',
        libraryTarget: 'window'
    },
    mode: 'production',
};