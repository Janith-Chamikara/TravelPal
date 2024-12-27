const { MongoClient } = require("mongodb");
const XLSX = require("xlsx");

// Configuration - update these values
const MONGODB_URI = "mongodb://localhost:27017";
const DATABASE_NAME = "your_database_name";
const EXCEL_FILE_PATH = "./categories.xlsx"; // Update with your Excel file path
const SHEET_NAME = "Sheet1"; // Update with your sheet name

async function readExcelFile() {
  try {
    // Read the Excel file
    const workbook = XLSX.readFile(EXCEL_FILE_PATH);
    const worksheet = workbook.Sheets[SHEET_NAME];

    // Convert to JSON
    const data = XLSX.utils.sheet_to_json(worksheet, {
      raw: true,
      defval: null,
    });

    // Transform data to match MongoDB schema
    return data
      .map((row) => ({
        _id: row["Category ID"],
        label: row["Category Label"],
      }))
      .filter((item) => item._id && item.label); // Remove any invalid entries
  } catch (error) {
    console.error("Error reading Excel file:", error);
    throw error;
  }
}

async function seedDatabase() {
  let client;

  try {
    // Read data from Excel
    console.log("Reading Excel file...");
    const categories = await readExcelFile();
    console.log(`Found ${categories.length} categories in Excel file`);

    // Connect to MongoDB
    client = await MongoClient.connect(MONGODB_URI);
    console.log("Connected to MongoDB");

    const db = client.db(DATABASE_NAME);
    const collection = db.collection("preferences");

    // Drop existing collection if it exists
    await collection.drop().catch((err) => {
      if (err.code !== 26) {
        // Error code 26 means collection doesn't exist
        throw err;
      }
    });
    console.log("Dropped existing collection");

    // Insert the categories
    const result = await collection.insertMany(categories);
    console.log(`Successfully inserted ${result.insertedCount} categories`);

    // Optional: Create an index on the label field for better query performance
    await collection.createIndex({ label: 1 });
    console.log("Created index on label field");
  } catch (error) {
    console.error("Error seeding database:", error);
    throw error;
  } finally {
    if (client) {
      await client.close();
      console.log("Database connection closed");
    }
  }
}

// Execute the script
seedDatabase()
  .then(() => console.log("Seeding completed successfully"))
  .catch((error) => {
    console.error("Seeding failed:", error);
    process.exit(1);
  });
