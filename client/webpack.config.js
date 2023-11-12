const path = require('path');
const TerserPlugin = require('terser-webpack-plugin');

module.exports = [
  // CommonJS (for Node.js)
  {
    mode: 'production',
    entry: './src/index.ts', // Point to your index.ts
    target: 'node',
    output: {
      path: path.resolve(__dirname, 'dist'),
      filename: 'index.cjs.js', // Output name for CommonJS
      libraryTarget: 'commonjs2'
    },
    module: {
      rules: [
        {
          test: /\.ts$/,
          use: 'ts-loader',
          exclude: /node_modules/
        }
      ]
    },
    resolve: {
      extensions: ['.ts', '.js']
    }
  },
  // ESM (for modern browsers and ES6+ environments)
  {
    mode: 'production',
    entry: './src/index.ts', // Point to your index.ts
    target: 'web',
    output: {
      path: path.resolve(__dirname, 'dist'),
      filename: 'index.esm.js', // Output name for ESM
      library: {
        type: 'module'
      }
    },
    experiments: {
      outputModule: true
    },
    module: {
      rules: [
        {
          test: /\.ts$/,
          use: 'ts-loader',
          exclude: /node_modules/
        }
      ]
    },
    resolve: {
      extensions: ['.ts', '.js']
    }
  },
  // UMD (for universal compatibility)
  {
    mode: 'production',
    entry: './src/index.ts', // Point to your index.ts
    target: 'web',
    output: {
      path: path.resolve(__dirname, 'dist'),
      filename: 'index.umd.js', // Output name for UMD
      library: {
        name: 'GridifyQueryBuilder',
        type: 'umd'
      },
      globalObject: 'this'
    },
    module: {
      rules: [
        {
          test: /\.ts$/,
          use: 'ts-loader',
          exclude: /node_modules/
        }
      ]
    },
    resolve: {
      extensions: ['.ts', '.js']
    }
  }
];

