import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import typescript from '@rollup/plugin-typescript';
import { terser } from 'rollup-plugin-terser';

export default [
  // ESM bundle
  {
    input: 'src/index.ts',
    output: {
      file: 'dist/index.esm.js',
      format: 'esm',
      sourcemap: true
    },
    plugins: [
      typescript(),
      resolve(),
      commonjs(),
      terser() // Uncomment if you want to minify the output
    ]
  },
  // CommonJS bundle
  {
    input: 'src/index.ts',
    output: {
      file: 'dist/index.cjs.js',
      format: 'cjs',
      sourcemap: true
    },
    plugins: [
      typescript(),
      resolve(),
      commonjs(),
      terser() // Uncomment if you want to minify the output
    ]
  },
  // UMD bundle
  {
    input: 'src/index.ts',
    output: {
      file: 'dist/index.umd.js',
      format: 'umd',
      name: 'GridifyClient', // The global variable name in browser
      sourcemap: true
    },
    plugins: [
      typescript(),
      resolve(),
      commonjs(),
      terser() // Uncomment if you want to minify the output
    ]
  }
];
