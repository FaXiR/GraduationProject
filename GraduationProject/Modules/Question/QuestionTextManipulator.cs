using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GraduationProject.Modules.Question
{
    class QuestionTextManipulator
    {
        private string AnswerBreak = "|/aNs-QTM/|";
        private string QuestionBreak = "|/qEb-QTM/|";

        public string QuestionListResultToString(List<Question> QuestionList)
        {
            string result = "";
            foreach (Question quest in QuestionList)
            {
                result += quest.selectAnswer.ToString();
                result += QuestionBreak;
            }
            return result;
        }

        public List<Question> StringResultToResultToQuestionList(List<Question> QuestionList, string ResultString)
        {
            string[] QB = new string[] { QuestionBreak };

            string[] indexMass = ResultString.Split(QB, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < QuestionList.Count; i++)
            {
                QuestionList[i].selectAnswer = Convert.ToInt32(indexMass[i]);
            }

            return QuestionList;
        }

        public string QuestionListToString(List<Question> QuestionList)
        {
            string result = "";
            foreach (Question quest in QuestionList)
            {
                result += quest.QuestionText + AnswerBreak;
                foreach (Answer answer in quest.AnswerList)
                {
                    result += answer.Text + AnswerBreak;
                }
                result += QuestionBreak;
            }
            return result;
        }

        public List<Question> StringToQuestionList(string QuestionString)
        {
            string[] QB = new string[] { QuestionBreak };
            string[] AB = new string[] { AnswerBreak };

            List<Question> result = new List<Question>();

            string[] QuestionList = QuestionString.Split(QB, StringSplitOptions.RemoveEmptyEntries);
            foreach (string FullQuestion in QuestionList)
            {
                List<string> QuestMass = new List<string>();
                QuestMass.AddRange(FullQuestion.Split(AB, StringSplitOptions.RemoveEmptyEntries));

                string Quest = "";
                if (QuestMass.Count != 0)
                {
                    Quest = QuestMass[0];
                    QuestMass.RemoveAt(0);
                }

                result.Add(new Question(Quest, QuestMass));
            }

            return result;
        }
    }
}
