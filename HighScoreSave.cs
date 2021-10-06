using System;
using System.Text;
using TMPro;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class HighScore
{
    public string[] names;
    public int[] scores;
}

public static class HighScoreSave
{
    static readonly string path = Path.Combine(Application.persistentDataPath, "scores.dat");

    public static void Save(string name, int score)
    {
        name = name.ToLower();
        HighScore dataToWrite = new HighScore();
        BinaryFormatter formatter = new BinaryFormatter();

        if (!File.Exists(path))
        {
            dataToWrite.names = new string[1] {name};
            dataToWrite.scores = new int[1] {score};
            
            // Create a new file and write the current score/name to it
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(stream, dataToWrite);
            }
        }
        else
        {
            // Open the file and save it to data variable
            HighScore data;
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                data = (HighScore) formatter.Deserialize(stream);
            }
            
            // Check to see if the input name matches a name on file, then check if the input score is higher than the score in the file saved, in that case override the current saved score
            for (int i = 0; i < data.names.Length; i++)
            {
                if (data.names[i] == name)
                {
                    if (score > data.scores[i])
                    {
                        data.scores[i] = score;
                        using (FileStream stream = new FileStream(path, FileMode.Create))
                        {
                            formatter.Serialize(stream, data);
                        }
                    }
                    return;
                }
            }

            // Create a new HighScore data type with the storage amount of the names/scores on file + one more for the currently input data
            dataToWrite.names = new string[data.names.Length + 1];
            dataToWrite.scores = new int[data.scores.Length + 1];
            
            for (int i = 0; i < data.names.Length; i++)
            {
                dataToWrite.names[i] = data.names[i];
                dataToWrite.scores[i] = data.scores[i];
            }

            dataToWrite.names[dataToWrite.names.Length - 1] = name;
            dataToWrite.scores[dataToWrite.scores.Length - 1] = score;
            
            // Replace the old file with the current dataToWrite variable
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(stream, dataToWrite);
            }
        }
    }

    public static HighScore Load()
    {
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            HighScore data;
            
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                data = (HighScore) formatter.Deserialize(stream);
            }
            
            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }

    public static void UpdateLeaderBoard(ref TMP_Text leaderboard)
    {
        HighScore scores = Load();

        if (scores != null)
        {
            for (int i = 0; i < scores.names.Length; i++)
            {
                int lowestScore = scores.scores[i];
                int lowestIndex = i;

                for (int j = i; j < scores.names.Length; j++)
                {
                    int score = scores.scores[j];

                    if (score < lowestScore)
                    {
                        lowestScore = score;
                        lowestIndex = j;
                    }
                
                    int temp = scores.scores[i];
                    string tempName = scores.names[i];
                
                    scores.scores[i] = scores.scores[lowestIndex];
                    scores.names[i] = scores.names[lowestIndex];
                
                    scores.scores[lowestIndex] = temp;
                    scores.names[lowestIndex] = tempName;
                }
            }
            
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < scores.names.Length; i++)
            {
                if (i + 1 > 10)
                {
                    break;
                }
                stringBuilder.Append($"{i + 1}: {scores.names[scores.names.Length - 1 - i]} - {scores.scores[scores.scores.Length - 1 - i]}\n");
            }

            leaderboard.text = stringBuilder.ToString();
        }
        else
        {
            leaderboard.text = "";
        }
    }

    public static void Clear(ref TMP_Text leaderboard)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            UpdateLeaderBoard(ref leaderboard);
        }
    }
}