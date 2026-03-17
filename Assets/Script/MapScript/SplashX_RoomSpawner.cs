using UnityEngine;
using System.Collections.Generic;

public class SplashX_RoomSpawner : MonoBehaviour
{
    public GameObject startRoom;
    public GameObject[] middleRooms;
    public GameObject endRoom;

    public int roomCount = 5;

    private Queue<GameObject> recentRooms = new Queue<GameObject>();
    public int noRepeatCount = 3;

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        GameObject currentRoom = Instantiate(startRoom, Vector3.zero, Quaternion.identity);

        Transform exitPoint = currentRoom.transform.Find("ExitPoint");

        for (int i = 0; i < roomCount; i++)
        {
            GameObject randomRoom = GetRandomRoom();

            GameObject newRoom = Instantiate(randomRoom);

            Transform entry = newRoom.transform.Find("EntryPoint");
            Transform exit = newRoom.transform.Find("ExitPoint");

            Vector3 offset = exitPoint.position - entry.position;
            newRoom.transform.position += offset;

            exitPoint = exit;

            recentRooms.Enqueue(randomRoom);

            if (recentRooms.Count > noRepeatCount)
                recentRooms.Dequeue();
        }

        GameObject finalRoom = Instantiate(endRoom);

        Transform finalEntry = finalRoom.transform.Find("EntryPoint");

        Vector3 finalOffset = exitPoint.position - finalEntry.position;
        finalRoom.transform.position += finalOffset;
    }

    GameObject GetRandomRoom()
    {
        List<GameObject> availableRooms = new List<GameObject>();

        foreach (var room in middleRooms)
        {
            if (!recentRooms.Contains(room))
                availableRooms.Add(room);
        }

        if (availableRooms.Count == 0)
            availableRooms.AddRange(middleRooms);

        return availableRooms[Random.Range(0, availableRooms.Count)];
    }
}