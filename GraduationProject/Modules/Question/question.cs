using System;
using System.Collections.Generic;

namespace GraduationProject.Modules.Question
{
    /// <summary>
    /// Формирования вопроса с ответами
    /// </summary>
    class question
    {
        /// <summary>
        /// Статус сохраненности вопроса. Задавайте False, если вы сохранили изменения.
        /// </summary>
        public bool IsEdited { get; set; } = true;

        /// <summary>
        /// Выбранный вариант(индекс) ответа
        /// </summary>
        public int selectAnswer { get; private set; } = -1;

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
        public List<string> AnswerList
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
        private List<string> answerList = new List<string>();

        /// <summary>
        /// Используется для формирования вопроса с ответами
        /// </summary>
        /// <param name="questionText">Вопросительное предложение</param>
        /// <param name="answerListText">Варианты ответов</param>
        public question(string questionText, List<string> answerListText)
        {
            this.questionText = questionText;
            this.answerList.AddRange(answerListText);
        }

        /// <summary>
        /// Используется для формирования вопроса с ответами
        /// </summary>
        /// <param name="questionText">Вопросительное предложение</param>
        /// <param name="answerListText">Варианты ответов</param>
        public question(string questionText, params string[] answerListText)
        {
            this.questionText = questionText;
            this.answerList.AddRange(answerListText);
        }

        /// <summary>
        /// Добавляет варианты ответа
        /// </summary>
        /// <param name="answerText">Варианты ответа</param>
        public void AddAnswer(params string[] answerText)
        {
            IsEdited = true;

            foreach (string answ in answerText)
            {
                answerList.Add(answ);
            }
        }

        /// <summary>
        /// Удаление вариантов ответа
        /// </summary>
        /// <param name="answerText">Текст удаляемых ответа</param>
        public void DeleteAnswer(params string[] answerText)
        {
            foreach (string answ in answerText)
            {
                if (answerList.Remove(answ))
                {
                    IsEdited = true;
                }
                else
                {
                    throw new KeyNotFoundException($@"Значение ""{answ}"" не найдено");
                }
            }
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

        /// <summary>
        /// Задает индекс выбранно ответа 
        /// </summary>
        /// <param name="answerText">Текст ответа</param>
        public void SelectAnswer(string answerText)
        {
            if (answerText == null)
            {
                selectAnswer = -1;
                return;
            }

            int selectAnswerTemp = answerList.IndexOf(answerText);
            if (selectAnswerTemp == -1)
            {
                throw new KeyNotFoundException($@"Варант ответа ""{answerText}"" для удаления не найден");
            }

            selectAnswer = selectAnswerTemp;
        }
    }
}
