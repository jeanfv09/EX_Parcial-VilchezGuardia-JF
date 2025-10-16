Service ID: srv-d3gp9s7fte5s73cc80b0
Link de la pagina web en render:
[https://ex-parcial-vilchezguardia-jf-7.onrender.com](https://ex-parcial-vilchezguardia-jf-7.onrender.com)
https://ex-parcial-vilchezguardia-jf-7.onrender.com

tal vez en el campus este mal conectado

# EX_Parcial-VilchezGuardia-JF

## Project Overview
This project is a Node.js application that serves as a backend server. It utilizes Express.js for routing and handling requests. The application is structured to separate concerns, with controllers managing business logic, routes defining the API endpoints, and utility functions providing helper methods.

## File Structure
```
EX_Parcial-VilchezGuardia-JF
├── src
│   ├── app.js
│   ├── controllers
│   │   └── index.js
│   ├── routes
│   │   └── index.js
│   └── utils
│       └── index.js
├── Dockerfile
├── .dockerignore
├── package.json
└── README.md
```

## Setup Instructions

1. **Clone the Repository**
   ```bash
   git clone https://github.com/jeanfv09/EX_Parcial-VilchezGuardia-JF
   cd EX_Parcial-VilchezGuardia-JF
   ```

2. **Install Dependencies**
   Make sure you have Node.js installed. Then run:
   ```bash
   npm install
   ```

3. **Run the Application**
   You can start the application using:
   ```bash
   npm start
   ```

4. **Docker Setup**
   To build and run the application using Docker, execute the following commands:
   ```bash
   docker build -t your-image-name .
   docker run -p 3000:3000 your-image-name
   ```

## Usage
Once the application is running, you can access the API endpoints defined in the routes. Use tools like Postman or curl to interact with the API.

## Contributing
Feel free to fork the repository and submit pull requests for any improvements or bug fixes.

## License
This project is licensed under the MIT License.
