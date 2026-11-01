using System;
using UnityEngine;

namespace Game.Map3
{
    [Serializable]
    public class Map3_QuizData
    {
        public string id;
        public string title;
        [TextArea(3, 8)]
        public string question;
        public string[] options;
        public int correctIndex;
        [TextArea(3, 8)]
        public string explanation;

        public Map3_QuizData(string id, string title, string question, string optA, string optB, string optC, string optD, int correctIndex, string explanation)
        {
            this.id = id;
            this.title = title;
            this.question = question;
            this.options = new string[] { optA, optB, optC, optD };
            this.correctIndex = correctIndex;
            this.explanation = explanation;
        }
    }
}
