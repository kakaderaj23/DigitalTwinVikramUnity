using UnityEngine;
using MongoDB.Bson;
using MongoDB.Driver;
using TMPro;
using System.Collections;

public class LatheMachineManager : MonoBehaviour
{
    [Header("MongoDB Settings")]
    public string mongoURI = "mongodb+srv://kakaderaj23:uZ99p79aNhMH1wwL@cluster0.o7rka2j.mongodb.net/";
    [Tooltip("e.g., LATHE-05, LATHE-08")]
    public string machineId = "LATHE-05";

    [Header("UI Elements")]
    public GameObject jobDetailsWindow;
    public TextMeshProUGUI latheIdText;           // Label above job panel (now shows Machine ID)
    public TextMeshProUGUI jobDetailsText;
    public TextMeshProUGUI LatheLabelText;        // Big label on kiosk/card
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
    public UnityEngine.UI.Slider remainingTimeSlider; // kept but unused

    [Header("Scene Objects")]
    public GameObject workerGameObject;      // worker visual
    public GameObject latheAnimationObject;  // machine animation

    // MongoDB
    private IMongoCollection<BsonDocument> jobCollection;
    private IMongoCollection<BsonDocument> sensoryCollection;

    // Toggle this to move to centralized DB naming if/when you switch
    [Header("DB Layout")]
    public bool useNewDbLayout = false; // false => old per-lathe DB; true => central DBs

    // Treat these as “active” statuses. Add more if your pipeline uses others.
    private static readonly string[] ActiveStatuses = { "running", "started" };

    void Start()
    {
        ConnectMongoDB();

        if (workerGameObject) workerGameObject.SetActive(false);
        if (latheAnimationObject) latheAnimationObject.SetActive(false);

        // Wire up buttons
        openDetailsButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OpenJobDetails);
        closeJobDetailsButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(CloseJobDetails);
        showSensoryDataButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OpenSensoryData);
        closeSensoryDataButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(CloseSensoryData);

        // Labels
        if (LatheLabelText) LatheLabelText.SetText($"Lathe {machineId}");
        if (latheIdText) latheIdText.text = $"Machine ID : {machineId}";

        // Panels
        jobDetailsWindow.SetActive(false);
        sensoryDataWindow.SetActive(false);

        // Outer buttons state
        openDetailsButtonOuter.SetActive(true);
        closeJobDetailsButtonOuter.SetActive(false);
        showSensoryDataButtonOuter.SetActive(false);

        if (remainingTimeSlider != null) remainingTimeSlider.gameObject.SetActive(false);

        StartCoroutine(MonitorMachineStatusRoutine());
    }

    void ConnectMongoDB()
    {
        var client = new MongoClient(mongoURI);

        if (!useNewDbLayout)
        {
            // Old layout: DB per-lathe, collections named JobDetails and SensoryData
            var database = client.GetDatabase("Lathe" + machineId); // if you used numeric earlier, consider changing to machineId
            jobCollection = database.GetCollection<BsonDocument>("JobDetails");
            sensoryCollection = database.GetCollection<BsonDocument>("SensoryData");
        }
        else
        {
            // Centralized layout (recommended going forward)
            var alertsDatabase  = client.GetDatabase("Alerts");
            var jobsDatabase    = client.GetDatabase("Jobs");
            var sensoryDatabase = client.GetDatabase("SensorData");

            // Example naming; adjust if your actual collection names differ
            jobCollection     = jobsDatabase.GetCollection<BsonDocument>("lathe_jobs");
            sensoryCollection = sensoryDatabase.GetCollection<BsonDocument>("lathe_sensory_data");
            // var alerts = alertsDatabase.GetCollection<BsonDocument>("lathe_alerts");
        }
    }

    void OpenJobDetails()
    {
        jobDetailsWindow.SetActive(true);
        sensoryDataWindow.SetActive(false);

        if (latheIdText) latheIdText.text = $"Machine ID : {machineId}";

        openDetailsButtonOuter.SetActive(false);
        closeJobDetailsButtonOuter.SetActive(true);
        showSensoryDataButtonOuter.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(UpdateJobDetailsRoutine());
        StartCoroutine(MonitorMachineStatusRoutine()); // keep status monitoring on
    }

    void CloseJobDetails()
    {
        jobDetailsWindow.SetActive(false);
        sensoryDataWindow.SetActive(false);

        openDetailsButtonOuter.SetActive(true);
        closeJobDetailsButtonOuter.SetActive(false);
        showSensoryDataButtonOuter.SetActive(false);

        StopAllCoroutines();
        StartCoroutine(MonitorMachineStatusRoutine()); // keep status monitoring even when closed
    }

    void OpenSensoryData()
    {
        sensoryDataWindow.SetActive(true);
        StopCoroutineSafe(UpdateSensoryDataRoutine());
        StartCoroutine(UpdateSensoryDataRoutine());
    }

    void CloseSensoryData()
    {
        sensoryDataWindow.SetActive(false);
        StopCoroutineSafe(UpdateSensoryDataRoutine());
    }

    IEnumerator UpdateJobDetailsRoutine()
    {
        while (true)
        {
            yield return FetchAndDisplayJobDetails();
            yield return new WaitForSeconds(5f);
        }
    }

    IEnumerator UpdateSensoryDataRoutine()
    {
        while (true)
        {
            yield return FetchAndDisplaySensoryData();
            yield return new WaitForSeconds(5f);
        }
    }

    IEnumerator MonitorMachineStatusRoutine()
    {
        while (true)
        {
            yield return CheckMachineStatus();
            yield return new WaitForSeconds(3f);
        }
    }

    // --- JOBS ---

    IEnumerator FetchAndDisplayJobDetails()
    {
        // Try to get the most recent ACTIVE job first
        var activeFilter =
            Builders<BsonDocument>.Filter.Eq("machineId", machineId) &
            Builders<BsonDocument>.Filter.In("status", ActiveStatuses);

        var latestActive = jobCollection
            .Find(activeFilter)
            .Sort(Builders<BsonDocument>.Sort.Descending("startTime"))
            .Limit(1)
            .FirstOrDefault();

        BsonDocument docToShow = latestActive;

        // If none active, fall back to most recent job (any status) for this machine
        if (docToShow == null)
        {
            var anyFilter = Builders<BsonDocument>.Filter.Eq("machineId", machineId);
            docToShow = jobCollection
                .Find(anyFilter)
                .Sort(Builders<BsonDocument>.Sort.Descending("startTime"))
                .Limit(1)
                .FirstOrDefault();
        }

        if (docToShow != null)
        {
            string jobId          = SafeStr(docToShow, "jobId");
            string jobType        = SafeStr(docToShow, "jobType");
            string jobDescription = SafeStr(docToShow, "jobDescription");
            string operatorId     = SafeStr(docToShow, "operatorId");
            string status         = SafeStr(docToShow, "status");
            string error          = SafeStr(docToShow, "error");

            string startTimeStr = SafeDateStr(docToShow, "startTime");
            string endTimeStr   = SafeDateStr(docToShow, "endTime");

            string estimatedStr = SafeNumberStr(docToShow, "estimatedTime");
            string actualStr    = SafeNumberStr(docToShow, "actualDuration");

            jobDetailsText.text =
                $"Job ID : {jobId}\n" +
                $"Job Type : {jobType}\n" +
                $"Job Description : {jobDescription}\n" +
                $"Operator : {operatorId}\n" +
                $"Start Time : {startTimeStr}\n" +
                $"End Time : {endTimeStr}\n" +
                $"Estimated Time : {estimatedStr} min\n" +
                $"Actual Duration : {actualStr} min\n" +
                $"Status : {status}" +
                (string.IsNullOrWhiteSpace(error) ? "" : $"\nError : {error}");

            // show sensory button always; or restrict only when active
            showSensoryDataButtonOuter.SetActive(true);
        }
        else
        {
            jobDetailsText.text = "No jobs found for this machine.";
            showSensoryDataButtonOuter.SetActive(false);
        }

        yield break;
    }

    // --- SENSORS ---

    IEnumerator FetchAndDisplaySensoryData()
    {
        var filter = Builders<BsonDocument>.Filter.Eq("machineId", machineId);

        var doc = sensoryCollection
            .Find(filter)
            .Sort(Builders<BsonDocument>.Sort.Descending("timestamp"))
            .Limit(1)
            .FirstOrDefault();

        if (doc != null)
        {
            string tsStr = SafeDateStr(doc, "timestamp");

            string airTemp         = SafeNumberStr(doc, "airTemperature");
            string procTemp        = SafeNumberStr(doc, "processTemperature");
            string rpm             = SafeNumberStr(doc, "rotationalSpeed");
            string torque          = SafeNumberStr(doc, "torque");
            string toolWear        = SafeNumberStr(doc, "toolWear");
            string failProb        = SafeNumberStr(doc, "failureProbability");

            sensoryDataText.text =
                $"Timestamp : {tsStr}\n" +
                $"Air Temperature : {airTemp}\n" +
                $"Process Temperature : {procTemp}\n" +
                $"Rotational Speed : {rpm}\n" +
                $"Torque : {torque}\n" +
                $"Tool Wear : {toolWear}\n" +
                $"Failure Probability : {failProb}";
        }
        else
        {
            sensoryDataText.text = "No sensory data found for this machine.";
        }

        yield break;
    }

    // --- STATUS / VISUALS ---

    IEnumerator CheckMachineStatus()
    {
        // Machine considered “working” if there’s an active job
        var activeFilter =
            Builders<BsonDocument>.Filter.Eq("machineId", machineId) &
            Builders<BsonDocument>.Filter.In("status", ActiveStatuses);

        var activeDoc = jobCollection.Find(activeFilter).Limit(1).FirstOrDefault();
        bool isWorking = activeDoc != null;

        if (workerGameObject)      workerGameObject.SetActive(isWorking);
        if (latheAnimationObject)  latheAnimationObject.SetActive(isWorking);

        yield break;
    }

    // --- Helpers ---

    private static string SafeStr(BsonDocument doc, string key, string fallback = "N/A")
    {
        if (!doc.Contains(key)) return fallback;
        var v = doc[key];
        return v == null || v.IsBsonNull ? fallback : v.ToString();
    }

    private static string SafeNumberStr(BsonDocument doc, string key, string fallback = "N/A")
    {
        if (!doc.Contains(key)) return fallback;
        var v = doc[key];
        if (v == null || v.IsBsonNull) return fallback;

        if (v.IsInt32)  return v.AsInt32.ToString();
        if (v.IsInt64)  return v.AsInt64.ToString();
        if (v.IsDouble) return v.AsDouble.ToString("0.###");
        if (v.IsDecimal128) return v.AsDecimal128.ToString();
        if (double.TryParse(v.ToString(), out double d)) return d.ToString("0.###");
        return v.ToString();
    }

    private static string SafeDateStr(BsonDocument doc, string key, string fallback = "—")
    {
        if (!doc.Contains(key)) return fallback;
        var v = doc[key];
        if (v == null || v.IsBsonNull) return fallback;

        // Handles { "$date": ... } as BsonDateTime or ISO8601 strings
        if (v.IsBsonDateTime) return v.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
        if (v.IsBsonDocument && v.AsBsonDocument.Contains("$date"))
        {
            var dateVal = v.AsBsonDocument["$date"];
            if (dateVal.IsBsonDateTime) return dateVal.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
            if (dateVal.IsString && System.DateTime.TryParse(dateVal.AsString, out var dt))
                return dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
        }
        if (v.IsString && System.DateTime.TryParse(v.AsString, out var dt2))
            return dt2.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'");

        return fallback;
    }

    private void StopCoroutineSafe(IEnumerator routine)
    {
        if (routine == null) return;
        try { StopCoroutine(routine); } catch { /* ignore */ }
    }
}
