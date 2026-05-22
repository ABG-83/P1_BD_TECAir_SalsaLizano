const { getDefaultConfig } = require('expo/metro-config');

/** @type {import('expo/metro-config').MetroConfig} */
const config = getDefaultConfig(__dirname);

// Agregamos soporte para archivos .wasm para que expo-sqlite funcione en la web
config.resolver.assetExts.push('wasm');

module.exports = config;
