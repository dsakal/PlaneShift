using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Manager : MonoBehaviour
{
    private Move Move;
    public FacingDirection facingDirection;
    public GameObject Player;
    private float degree = 0;
    public Transform Level;
    public Transform Building;
    public GameObject InvisiCube;
    private List<Transform> InvisiList = new List<Transform>();
    private FacingDirection lastfacing;
    private float lastDepth = 0f;
    public float WorldUnits = 1.000f;

    void Start()
    {
        facingDirection = FacingDirection.Front;
        Move = Player.GetComponent<Move>();
        UpdateLevelData(true);
    }

    void Update()
    {

        if (!Move._jumping)
        {
            bool updateData = false;
            if (OnInvisiblePlatform())
                if (MovePlayerDepthToClosestPlatform())
                    updateData = true;
            if (MoveToClosestPlatformToCamera())
                updateData = true;
            if (updateData)
                UpdateLevelData(false);


        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (OnInvisiblePlatform())
            {
                MovePlayerDepthToClosestPlatform();

            }
            lastfacing = facingDirection;
            facingDirection = RotateDirectionRight();
            degree -= 90f;
            UpdateLevelData(false);
            Move.UpdateToFacingDirection(facingDirection, degree);



        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            if (OnInvisiblePlatform())
            {
                MovePlayerDepthToClosestPlatform();

            }
            lastfacing = facingDirection;
            facingDirection = RotateDirectionLeft();
            degree += 90f;
            UpdateLevelData(false);
            Move.UpdateToFacingDirection(facingDirection, degree);


        }
    }

    private void UpdateLevelData(bool forceRebuild)
    {
        if (!forceRebuild)
            if (lastfacing == facingDirection && lastDepth == GetPlayerDepth())
                return;
        foreach (Transform tr in InvisiList)
        {

            tr.position = Vector3.zero;
            Destroy(tr.gameObject);

        }
        InvisiList.Clear();
        float newDepth = 0f;

        newDepth = GetPlayerDepth();
        CreateInvisicubesAtNewDepth(newDepth);

    }

    private bool OnInvisiblePlatform()
    {
        foreach (Transform item in InvisiList)
        {

            if (Mathf.Abs(item.position.x - Move.transform.position.x) < WorldUnits && Mathf.Abs(item.position.z - Move.transform.position.z) < WorldUnits)
                if (Move.transform.position.y - item.position.y <= WorldUnits + 0.2f && Move.transform.position.y - item.position.y > 0)
                    return true;



        }
        return false;
    }

    private bool MoveToClosestPlatformToCamera()
    {
        bool moveCloser = false;
        foreach (Transform item in Level)
        {
            if (facingDirection == FacingDirection.Front || facingDirection == FacingDirection.Back)
            {

                if (Mathf.Abs(item.position.x - Move.transform.position.x) < WorldUnits + 0.1f)
                {

                    if (Move.transform.position.y - item.position.y <= WorldUnits + 0.2f && Move.transform.position.y - item.position.y > 0 && !Move._jumping)
                    {
                        if (facingDirection == FacingDirection.Front && item.position.z < Move.transform.position.z)
                            moveCloser = true;

                        if (facingDirection == FacingDirection.Back && item.position.z > Move.transform.position.z)
                            moveCloser = true;


                        if (moveCloser)
                        {

                            Move.transform.position = new Vector3(Move.transform.position.x, Move.transform.position.y, item.position.z);
                            return true;
                        }
                    }

                }

            }
            else
            {
                if (Mathf.Abs(item.position.z - Move.transform.position.z) < WorldUnits + 0.1f)
                {
                    if (Move.transform.position.y - item.position.y <= WorldUnits + 0.2f && Move.transform.position.y - item.position.y > 0 && !Move._jumping)
                    {
                        if (facingDirection == FacingDirection.Right && item.position.x > Move.transform.position.x)
                            moveCloser = true;

                        if (facingDirection == FacingDirection.Left && item.position.x < Move.transform.position.x)
                            moveCloser = true;

                        if (moveCloser)
                        {
                            Move.transform.position = new Vector3(item.position.x, Move.transform.position.y, Move.transform.position.z);
                            return true;
                        }

                    }

                }
            }


        }
        return false;
    }

    private bool FindTransformInvisiList(Vector3 cube)
    {
        foreach (Transform item in InvisiList)
        {
            if (item.position == cube)
                return true;
        }
        return false;

    }

    private bool FindTransformLevel(Vector3 cube)
    {
        foreach (Transform item in Level)
        {
            if (item.position == cube)
                return true;

        }
        return false;

    }

    private bool FindTransformBuilding(Vector3 cube)
    {
        foreach (Transform item in Building)
        {
            if (facingDirection == FacingDirection.Front)
            {
                if (item.position.x == cube.x && item.position.y == cube.y && item.position.z < cube.z)
                    return true;
            }
            else if (facingDirection == FacingDirection.Back)
            {
                if (item.position.x == cube.x && item.position.y == cube.y && item.position.z > cube.z)
                    return true;
            }
            else if (facingDirection == FacingDirection.Right)
            {
                if (item.position.z == cube.z && item.position.y == cube.y && item.position.x > cube.x)
                    return true;

            }
            else
            {
                if (item.position.z == cube.z && item.position.y == cube.y && item.position.x < cube.x)
                    return true;

            }
        }
        return false;

    }

    private bool MovePlayerDepthToClosestPlatform()
    {
        foreach (Transform item in Level)
        {

            if (facingDirection == FacingDirection.Front || facingDirection == FacingDirection.Back)
            {
                if (Mathf.Abs(item.position.x - Move.transform.position.x) < WorldUnits + 0.1f)
                    if (Move.transform.position.y - item.position.y <= WorldUnits + 0.2f && Move.transform.position.y - item.position.y > 0)
                    {

                        Move.transform.position = new Vector3(Move.transform.position.x, Move.transform.position.y, item.position.z);
                        return true;

                    }
            }
            else
            {
                if (Mathf.Abs(item.position.z - Move.transform.position.z) < WorldUnits + 0.1f)
                    if (Move.transform.position.y - item.position.y <= WorldUnits + 0.2f && Move.transform.position.y - item.position.y > 0)
                    {

                        Move.transform.position = new Vector3(item.position.x, Move.transform.position.y, Move.transform.position.z);
                        return true;
                    }
            }
        }
        return false;

    } 

    private Transform CreateInvisicube(Vector3 position)
    {
        GameObject go = Instantiate(InvisiCube) as GameObject;

        go.transform.position = position;

        return go.transform;

    }

    private void CreateInvisicubesAtNewDepth(float newDepth)
    {

        Vector3 tempCube = Vector3.zero;
        foreach (Transform child in Level)
        {

            if (facingDirection == FacingDirection.Front || facingDirection == FacingDirection.Back)
            {
                tempCube = new Vector3(child.position.x, child.position.y, newDepth);
                if (!FindTransformInvisiList(tempCube) && !FindTransformLevel(tempCube) && !FindTransformBuilding(child.position))
                {

                    Transform go = CreateInvisicube(tempCube);
                    InvisiList.Add(go);
                }

            }
            else if (facingDirection == FacingDirection.Right || facingDirection == FacingDirection.Left)
            {
                tempCube = new Vector3(newDepth, child.position.y, child.position.z);
                if (!FindTransformInvisiList(tempCube) && !FindTransformLevel(tempCube) && !FindTransformBuilding(child.position))
                {

                    Transform go = CreateInvisicube(tempCube);
                    InvisiList.Add(go);
                }

            }


        }


    }

    private float GetPlayerDepth()
    {
        float ClosestPoint = 0f;

        if (facingDirection == FacingDirection.Front || facingDirection == FacingDirection.Back)
        {
            ClosestPoint = Move.transform.position.z;

        }
        else if (facingDirection == FacingDirection.Right || facingDirection == FacingDirection.Left)
        {
            ClosestPoint = Move.transform.position.x;
        }


        return Mathf.Round(ClosestPoint);

    }

    private FacingDirection RotateDirectionRight()
    {
        int change = (int)(facingDirection);
        change++;
        if (change > 3)
            change = 0;
        return (FacingDirection)(change);
    }

    private FacingDirection RotateDirectionLeft()
    {
        int change = (int)(facingDirection);
        change--;
        if (change < 0)
            change = 3;
        return (FacingDirection)(change);
    }

}

public enum FacingDirection
{
    Front = 0,
    Right = 1,
    Back = 2,
    Left = 3

}