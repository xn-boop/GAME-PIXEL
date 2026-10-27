using System;
using UnityEngine;

[Serializable]
public class L1_QuizData
{
    public string id;
    public string title;
    [TextArea(3, 8)]
    public string question;
    public string[] options;
    public int correctIndex;
    [TextArea(2, 6)]
    public string explanation;

    public L1_QuizData(string id, string title, string question, string optA, string optB, string optC, string optD, int correctIndex, string explanation = "")
    {
        this.id = id;
        this.title = title;
        this.question = question;
        this.options = new string[] { optA, optB, optC, optD };
        this.correctIndex = correctIndex;
        this.explanation = explanation;
    }
}
