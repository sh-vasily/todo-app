const PROXY_CONFIG = [
  {
    context: [
      "/api",
    ],
    target: "http://localhost:8083",
    secure: false
  }
]

module.exports = PROXY_CONFIG;
