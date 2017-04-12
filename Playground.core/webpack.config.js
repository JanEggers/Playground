var HtmlWebpackPlugin = require('html-webpack-plugin');

module.exports = {
    output: {
        path: __dirname,
        filename: "bundle.js"
    },
    resolve: {
        extensions: ['','.ts', '.tsx', '.js', '.jsx']
    },
    devtool: 'source-map',
    module: {
        loaders: [
            { test: /\.ts?$/, loaders: ['awesome-typescript-loader', 'angular2-template-loader'] },
            { test: /\.json$/, loader: 'json-loader' },
        ]
    },

    plugins: [
        new HtmlWebpackPlugin({
            template: 'ClientApp/index.html',
            inject: 'body',
        })
    ]
};