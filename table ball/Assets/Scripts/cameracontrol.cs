using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public GameObject P1;  // 第一个视角（如第一人称）
    public GameObject P3;  // 第二个视角（如第三人称）
    private bool isFirstPersonActive = false;  // 更清晰的变量名

    void Start()
    {
        // 初始状态与变量一致
        P1.SetActive(isFirstPersonActive);
        P3.SetActive(!isFirstPersonActive);
    }

    void Update()
    {
        // 检测数字键0（主键盘）或小键盘0
        if (Input.GetKeyUp(KeyCode.Alpha0) || Input.GetKeyUp(KeyCode.Keypad0))
        {
            // 切换状态
            isFirstPersonActive = !isFirstPersonActive;
            
            // 更新摄像机状态
            P1.SetActive(isFirstPersonActive);
            P3.SetActive(!isFirstPersonActive);
        }
    }
}