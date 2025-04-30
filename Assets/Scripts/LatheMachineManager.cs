using UnityEngine;
using MongoDB.Bson;
using MongoDB.Driver;
using TMPro;
using System.Collections;
using System.IO;

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
    public GameObject openDetailsButtonOuter;
    public GameObject closeJobDetailsButtonOuter;
    public GameObject showSensoryDataButtonOuter;

    [Header("Slider")]
    public UnityEngine.UI.Slider remainingTimeSlider;  // KEEP IT but unused now

    // MongoDB references
    private IMongoCollection<BsonDocument> jobCollection;
    private IMongoCollection<BsonDocument> sensoryCollection;

    void Start()
    {
        ConnectMongoDB();

        openDetailsButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OpenJobDetails);
        closeJobDetailsButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(CloseJobDetails);
        showSensoryDataButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OpenSensoryData);

        Debug.Log("Button listeners set up");

        jobDetailsWindow.SetActive(false);
        sensoryDataWindow.SetActive(false);

        openDetailsButtonOuter.SetActive(true);
        closeJobDetailsButtonOuter.SetActive(false);
        showSensoryDataButtonOuter.SetActive(false);

        if (remainingTimeSlider != null)
            remainingTimeSlider.gameObject.SetActive(false);  // Keep it hidden always
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
        sensoryDataWindow.SetActive(false);

        latheIdText.text = "Lathe ID : Lathe" + latheId;

        openDetailsButtonOuter.SetActive(false);
        closeJobDetailsButtonOuter.SetActive(true);
        showSensoryDataButtonOuter.SetActive(true);

        StopCoroutine(UpdateJobDetailsRoutine());
        StartCoroutine(UpdateJobDetailsRoutine());
    }

    void CloseJobDetails()
    {
        jobDetailsWindow.SetActive(false);
        sensoryDataWindow.SetActive(false);

        openDetailsButtonOuter.SetActive(true);
        closeJobDetailsButtonOuter.SetActive(false);
        showSensoryDataButtonOuter.SetActive(false);

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
        var filter = Builders<BsonDocument>.Filter.Eq("Status", "Started");
        var sort = Builders<BsonDocument>.Sort.Descending("JobID");

        var document = await jobCollection.Find(filter).Sort(sort).FirstOrDefaultAsync();

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

            // Commented out progress bar update
            // UpdateProgressBar(document);
        }
        else
        {
            jobDetailsText.text = "No job is currently running.";
            // ResetProgressBar();
            showSensoryDataButtonOuter.SetActive(false);

            // Hide slider (even though it is already hidden)
            if (remainingTimeSlider != null)
                remainingTimeSlider.gameObject.SetActive(false);
        }
    }

    /*
    void UpdateProgressBar(BsonDocument jobDocument)
    {
        if (jobDocument.Contains("EstimatedTime") && jobDocument.Contains("StartTime"))
        {
            double estimatedTime = jobDocument.GetValue("EstimatedTime").ToDouble();
            var startTime = jobDocument.GetValue("StartTime").ToUniversalTime();
            double elapsedMinutes = (System.DateTime.UtcNow - startTime).TotalMinutes;

            double progress = Mathf.Clamp01((float)(elapsedMinutes / estimatedTime));
            remainingTimeSlider.value = (float)(1 - progress);
        }
    }

    void ResetProgressBar()
    {
        remainingTimeSlider.value = 0f;
    }
    */

    async void FetchAndDisplaySensoryData()
    {
        var document = await sensoryCollection.Find(new BsonDocument()).Sort(Builders<BsonDocument>.Sort.Descending("_id")).FirstOrDefaultAsync();

        if (document != null)
        {
            sensoryDataText.text =
                $"Temperature : {document.GetValue("Temperature", "N/A")} °C\n" +
                $"Vibration : {document.GetValue("Vibration", "N/A")} mm/s\n" +
                $"RPM : {document.GetValue("RPM", "N/A")}\n" +
                $"Power Consumption : {document.GetValue("Power", "N/A")} kW"+
                $"Tool Wear : {document.GetValue("ToolWear", "N/A")} %";
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
