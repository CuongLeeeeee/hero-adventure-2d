using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class SupabaseQuizAPI : MonoBehaviour
{
    string url = "https://oeipryfmgfgwccpaezxp.supabase.co/rest/v1/quiz?id=eq.";
    string apiKey = "sb_publishable_LMbWILp4lRFwPgZCn2nQ7w_UW4BIyEL";

    public IEnumerator GetQuiz(int id, Action<Quiz> callback)
    {
        string requestUrl = url + id;

        UnityWebRequest request = UnityWebRequest.Get(requestUrl);

        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            string json = request.downloadHandler.text;

            json = json.Substring(1, json.Length - 2); // remove []

            Quiz quiz = JsonUtility.FromJson<Quiz>(json);

            callback(quiz);
        }
    }
}