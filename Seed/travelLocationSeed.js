const { MongoClient, ObjectId } = require("mongodb");

// Connection URI
const uri =
    "mongodb+srv://janithchamikara2021:6ggVGzrWK3zCUlwc@cluster0.ci036.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
const client = new MongoClient(uri);

// Database and collection names
const dbName = "Cluster0";
const collectionName = "travelLocations";

// User ID (from your example)
const userId = "67c3d6694d65d7e9a236d966";

// Preference options
const preferences = [
    "4d4b7104d754a06370d81259", // Arts and Entertainment
    "4bf58dd8d48988d182941735", // Amusement Parks
    "5109983191d435c0d71c2bb1", // Attractions
    "4fceea171983d5d06c3e9823", // Aquariums
    "4bf58dd8d48988d1e1931735", // Arcades
    "4bf58dd8d48988d1e2931735", // Art Galleries
    "63be6904847c3692a84b9b20", // Bingo Centers
    "4bf58dd8d48988d1e4931735", // Bowling Alleys
    "63be6904847c3692a84b9b21", // Carnivals
];

// Sri Lankan locations and geographic data
const locationPrefixes = [
    "Colombo",
    "Kandy",
    "Galle",
    "Anuradhapura",
    "Polonnaruwa",
    "Nuwara Eliya",
    "Negombo",
    "Trincomalee",
    "Batticaloa",
    "Jaffna",
    "Matara",
    "Ratnapura",
    "Badulla",
    "Kurunegala",
    "Ella",
    "Sigiriya",
    "Dambulla",
    "Matale",
    "Kalutara",
    "Bentota",
    "Unawatuna",
    "Hikkaduwa",
    "Mirissa",
    "Tangalle",
    "Arugam Bay",
    "Kalpitiya",
    "Weligama",
    "Habarana",
    "Hambantota",
    "Kataragama",
];

const locationTypes = [
    "Beach",
    "Temple",
    "Park",
    "Hotel",
    "Restaurant",
    "Museum",
    "Garden",
    "Lake",
    "Mountain",
    "Waterfall",
    "Cave",
    "Fort",
    "Market",
    "Wildlife Sanctuary",
    "Ancient Ruins",
    "Tea Plantation",
    "Viewpoint",
    "Historical Site",
    "Village",
    "Bay",
    "Lagoon",
];

// Sri Lanka's approximate geographic boundaries
const sriLankaCoords = {
    minLat: 5.916,
    maxLat: 9.835,
    minLng: 79.652,
    maxLng: 81.881,
};

// Helper function to generate random location name
function generateLocationName() {
    const prefix =
        locationPrefixes[Math.floor(Math.random() * locationPrefixes.length)];
    const type = locationTypes[Math.floor(Math.random() * locationTypes.length)];
    const suffix = Math.floor(Math.random() * 100);
    return `${prefix} ${type} ${suffix}`;
}

// Helper function to generate random coordinates within Sri Lanka
function generateCoordinates() {
    const latitude =
        sriLankaCoords.minLat +
        Math.random() * (sriLankaCoords.maxLat - sriLankaCoords.minLat);
    const longitude =
        sriLankaCoords.minLng +
        Math.random() * (sriLankaCoords.maxLng - sriLankaCoords.minLng);
    return {
        latitude: parseFloat(latitude.toFixed(6)),
        longitude: parseFloat(longitude.toFixed(6)),
    };
}

// Helper function to get random preferences
function getRandomPreferences() {
    const numPreferences = Math.floor(Math.random() * 4) + 1; // 1 to 4 preferences
    const shuffled = [...preferences].sort(() => 0.5 - Math.random());
    return shuffled.slice(0, numPreferences);
}

// Helper function to generate random date within the last 3 years
function generateRandomDate() {
    const now = new Date();
    const threeYearsAgo = new Date();
    threeYearsAgo.setFullYear(now.getFullYear() - 3);

    // Random time between 3 years ago and now
    const randomTimestamp =
        threeYearsAgo.getTime() +
        Math.random() * (now.getTime() - threeYearsAgo.getTime());
    return new Date(randomTimestamp);
}

// Generate a single travel location
function generateTravelLocation() {
    const coords = generateCoordinates();
    return {
        _id: new ObjectId(),
        UserId: userId,
        LocationName: generateLocationName(),
        Latitude: coords.latitude,
        Longitude: coords.longitude,
        Preferences: getRandomPreferences(),
        CreatedAt: generateRandomDate(), // Using random date instead of current date
    };
}

// Main function to seed the database
async function seedDatabase() {
    try {
        // Connect to MongoDB
        await client.connect();
        console.log("Connected to MongoDB server");

        const db = client.db(dbName);
        const collection = db.collection(collectionName);

        // Generate and insert 6000 travel locations
        const totalLocations = 20000;
        const batchSize = 1000;
        let locationsCreated = 0;

        console.log(`Starting to seed ${totalLocations} travel locations...`);

        while (locationsCreated < totalLocations) {
            const batch = [];
            const currentBatchSize = Math.min(
                batchSize,
                totalLocations - locationsCreated
            );

            for (let i = 0; i < currentBatchSize; i++) {
                batch.push(generateTravelLocation());
            }

            await collection.insertMany(batch);
            locationsCreated += currentBatchSize;
            console.log(
                `Progress: ${locationsCreated}/${totalLocations} locations inserted`
            );
        }

        console.log("Database seeding completed successfully!");
    } catch (err) {
        console.error("Error seeding database:", err);
    } finally {
        await client.close();
        console.log("MongoDB connection closed");
    }
}

// Run the seeding function
seedDatabase().catch(console.error);
