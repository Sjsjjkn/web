const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  lintOnSave: false,
  devServer: {
    port: 8080,
    hot: true,
    liveReload: true,
    client: {
      webSocketURL: 'auto://0.0.0.0:0/ws',
      overlay: {
        warnings: false,
        errors: true
      }
    },
    proxy: {
      '/api': {
        target: 'http://localhost:5200',
        changeOrigin: true,
        pathRewrite: {
          '^/api': '/api'
        }
      }
    }
  }
})
