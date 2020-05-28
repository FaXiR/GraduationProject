using System;
using System.Collections.Generic;

namespace GraduationProject.Modules.Question
{
    /// <summary>
    /// Формирования вопроса с ответами
    /// </summary>
    class Question
    {
        /// <summary>
        /// Статус сохраненности вопроса. Задавайте False, если вы сохранили изменения.
        /// </summary>
        public bool IsEdited { get; set; } = true;

        /// <summary>
        /// Выбранный вариант(индекс) ответа
        /// </summary>
        public int selectAnswer { get; set; } = -1;

        /// <summary>
        /// Вопросительное предложение
        /// </summary>
        public string QuestionText
        {
            get
            {
                return questionText;
            }
            set
            {
                IsEdited = true;
                questionText = value;
            }
        }
        private string questionText;

        /// <summary>
        /// Варианты ответа
        /// </summary>
        public List<Answer> AnswerList
        {
            get
            {
                return answerList;
            }
            set
            {
                IsEdited = true;
                answerList = value;
            }
        }
        private List<Answer> answerList = new List<Answer>();

        /// <summary>
        /// Используется для формирования вопроса с ответами
        /// </summary>
        /// <param name="questionText">Вопросительное предложение</param>
        /// <param name="answerListText">Варианты ответов</param>
        public Question(string questionText, List<string> answerListText)
        {
            this.questionText = questionText;
            foreach (string answer in answerListText)
            {
                this.answerList.Add(new Answer(answer, null, null));
            }
        }

        /// <summary>
        /// Используется для формирования вопроса с ответами
        /// </summary>
        /// <param name="questionText">Вопросительное предложение</param>
        /// <param name="answerListText">Варианты ответов</param>
        public Question(string questionText, params string[] answerListText)
        {
            this.questionText = questionText;
            foreach (string answer in answerListText)
            {
                this.answerList.Add(new Answer(answer, null, null));
            }
        }

        /// <summary>
        /// Используется для формирования вопроса с ответами, используйте для формирования ответов с принадлежностью к группам
        /// </summary>
        /// <param name="questionText">Вопросительное предложение</param>
        public Question(string questionText)
        {
            this.questionText = questionText;
        }

        /// <summary>
        /// Добавляет варианты ответа
        /// </summary>
        /// <param name="answerText">Варианты ответа</param>
        public void AddAnswerList(params string[] answerText)
        {
            IsEdited = true;

            foreach (string answer in answerText)
            {
                this.answerList.Add(new Answer(answer, null, null));
            }
        }

        /// <summary>
        /// Добавляет вариант ответа
        /// </summary>
        /// <param name="answerText">Текст ответа</param>
        /// <param name="answerGroup">Группа ответа</param>
        /// <param name="answerVolue">Значение ответа</param>
        public void AddAnswerList(string answerText, string answerGroup, string answerVolue)
        {
            IsEdited = true;
            this.answerList.Add(new Answer(answerText, answerGroup, answerVolue));
        }

        /// <summary>
        /// Удаление варианта ответа
        /// </summary>
        /// <param name="answerIndex">Индекс удаляемого ответа</param>
        public void DeleteAnswer(int answerIndex)
        {
            if (answerIndex < 0 || answerIndex > answerList.Count - 1)
            {
                IsEdited = true;
                answerList.RemoveAt(answerIndex);
            }
            else
            {
                throw new IndexOutOfRangeException($@"Была попытка выбран индекс ""{answerIndex}"", из диапазона [0:{answerList.Count - 1}]");
            }
        }

        /// <summary>
        /// Задает индекс выбранно ответа 
        /// </summary>
        /// <param name="answerIndex">Индекс ответа</param>
        public void SelectAnswer(int answerIndex)
        {
            if (answerIndex == -1)
            {
                selectAnswer = -1;
                return;
            }
            else if (answerIndex < -1 || answerIndex > answerList.Count - 1)
            {
                throw new IndexOutOfRangeException($@"Была попытка выбран индекс ""{answerIndex}"", из диапазона [-1:{answerList.Count - 1}]");
            }

            selectAnswer = answerIndex;
        }
    }

    class Answer
    {
        public Answer(string text, string group, string volue)
        {
            Text = text;
            Group = group;
            Volue = volue;
        }

        public string Text { get; set; }
        public string Group { get; set; }
        public string Volue { get; set; }
    }
}
