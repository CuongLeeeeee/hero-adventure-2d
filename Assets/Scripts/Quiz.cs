using UnityEngine;
using System;

[Serializable]
public class Quiz
{
    public int id;
    public string question;
    public string[] options;
    public string correct_answer;
}
