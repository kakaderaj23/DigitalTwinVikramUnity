using UnityEngine;
using MongoDB.Bson;
using MongoDB.Driver;
using TMPro;
using System.Collections;

public class LatheMachineManager : MonoBehaviour
{
    [Header("MongoDB Settings")]
    public string mongoURI = "mongodb://localhost:27017"; // replace with your MongoDB URI
    public string latheId = "1"; // e.g., user sets this in the Inspector (1 or 2)

    [Header("UI Elements")]
    public GameObject jobDetailsWindow;
    public TextMeshProUGUI latheIdText;
    public TextMeshProUGUI jobDetailsText;
    public GameObject sensoryDataWindow;
    public TextMeshProUGUI sensoryDataText;

    [Header("Buttons")]
    public GameObject openDetailsButton;
    public GameObject closeJobDetailsButton;
    public GameObject showSensoryDataButton;
    public GameObject closeSensoryDataButton;

    // MongoDB references
    private IMongoCollection<BsonDocument> jobCollection;
    private IMongoCollection<BsonDocument> sensoryCollection;

    void Start()
    {
        // Initial setup: Connect to MongoDB
        ConnectMongoDB();
        Debug.Log("Connected to MongoDB");
        // Button listeners
        openDetailsButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OpenJobDetails);
        closeJobDetailsButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(CloseJobDetails);
        showSensoryDataButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OpenSensoryData);
        closeSensoryDataButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(CloseSensoryData);
        Debug.Log("Button listeners set up");
        // Initially hide windows
        jobDetailsWindow.SetActive(false);
        sensoryDataWindow.SetActive(false);
    }

    void ConnectMongoDB()
    {
        var client = new MongoClient(mongoURI);
        var database = client.GetDatabase("Lathe" + latheId);

        jobCollection = database.GetCollection<BsonDocument>("JobDetails");
        sensoryCollection = database.GetCollection<BsonDocument>("SensoryData");
    }

    void OpenJobDetails()
    {
        jobDetailsWindow.SetActive(true);
        latheIdText.text = "Lathe ID : Lathe" + latheId;
        StartCoroutine(UpdateJobDetailsRoutine());
    }

    void CloseJobDetails()
    {
        jobDetailsWindow.SetActive(false);
        sensoryDataWindow.SetActive(false);
        StopAllCoroutines();
    }

    void OpenSensoryData()
    {
        sensoryDataWindow.SetActive(true);
        StartCoroutine(UpdateSensoryDataRoutine());
    }

    void CloseSensoryData()
    {
        sensoryDataWindow.SetActive(false);
        StopCoroutine(UpdateSensoryDataRoutine());
    }

    IEnumerator UpdateJobDetailsRoutine()
    {
        while (true)
        {
            FetchAndDisplayJobDetails();
            yield return new WaitForSeconds(5f);
        }
    }

    IEnumerator UpdateSensoryDataRoutine()
    {
        while (true)
        {
            FetchAndDisplaySensoryData();
            yield return new WaitForSeconds(5f);
        }
    }

    async void FetchAndDisplayJobDetails()
    {
        var document = await jobCollection.Find(new BsonDocument()).Sort(Builders<BsonDocument>.Sort.Descending("_id")).FirstOrDefaultAsync();

        if (document != null)
        {
            jobDetailsText.text =
                $"Job ID : {document.GetValue("JobID", "N/A")}\n" +
                $"Job Type : {document.GetValue("JobType", "N/A")}\n" +
                $"Job Description : {document.GetValue("JobDescription", "N/A")}\n" +
                $"Material : {document.GetValue("Material", "N/A")}\n" +
                $"Tool No. : {document.GetValue("ToolNo", "N/A")}\n" +
                $"Start Time : {document.GetValue("StartTime", "N/A")}\n" +
                $"Estimated Time : {document.GetValue("EstimatedTime", "N/A")} min\n" +
                $"Operator Name : {document.GetValue("OperatorName", "N/A")}\n" +
                $"Status : {document.GetValue("Status", "N/A")}";
        }
        else
        {
            jobDetailsText.text = "No job details found.";
        }
    }


    async void FetchAndDisplaySensoryData()
    {
        var document = await sensoryCollection.Find(new BsonDocument()).Sort(Builders<BsonDocument>.Sort.Descending("_id")).FirstOrDefaultAsync();

        if (document != null)
        {
            sensoryDataText.text =
                $"Temperature : {document.GetValue("Temperature", "N/A")} °C\n" +
                $"Vibration : {document.GetValue("Vibration", "N/A")} mm/s\n" +
                $"RPM : {document.GetValue("RPM", "N/A")}\n" +
                $"Power Consumption : {document.GetValue("Power_Consumption", "N/A")} kW";
        }
        else
        {
            sensoryDataText.text = "No sensory data found.";
        }
    }

    void Update()
    {
        // Reserved for future updates or interactivity
    }
}
