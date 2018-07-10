using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
	public static TaskManager Instance = null;
	public Task[] tasks;
	public int currentTask;
	public GameObject tasksGrid;
	public string[] currentTasks;
	private void Awake()
	{
		Instance = this;
		tasks = tasksGrid.GetComponentsInChildren<Task>(true);
		for (int i = 0; i < tasks.Length; i++)
		{
			tasks[i].gameObject.SetActive(false);
		}
		LoadTasks();
	}
	void LoadTasks()
	{
		for (int i = 0; i < currentTasks.Length; i++)
		{
			tasks[i].gameObject.SetActive(true);
			tasks[i].ShowSelector(false);
			tasks[i].SetText(currentTasks[i]);
		}
		tasks[0].ShowSelector(true);
	}
	void HideAllTaskSelectors()
	{
		for (int i = 0; i < tasks.Length; i++)
		{
			tasks[i].ShowSelector(false);
		}
	}
	public void CompleteTask()
	{
		if (currentTask >= 0)
			tasks[currentTask].ShowSelector(false);
		currentTask++;
		if (currentTask < currentTasks.Length)
			tasks[currentTask].ShowSelector(true);
		if (currentTask >= currentTasks.Length)
		{
			ScreenManager.Instance.CloseAllScreens();
		}
	}
}
