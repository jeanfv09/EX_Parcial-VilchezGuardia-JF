const express = require('express');
const router = express.Router();
const { getUsers, createUser } = require('../controllers/index');

// Define routes
router.get('/users', getUsers);
router.post('/users', createUser);

// Export the router
module.exports = router;