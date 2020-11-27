﻿using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using ChessRules;

public class Board : MonoBehaviour
{
    Dictionary<string, GameObject> squares;
    Dictionary<string, GameObject> figures;
    Dictionary<string, GameObject> promots;
    DragAndDrop dad;
    Chess chess;
    string onPromotionMove;
    public Board()
    {
        squares = new Dictionary<string, GameObject>();
        figures = new Dictionary<string, GameObject>();
        promots = new Dictionary<string, GameObject>();
        chess = new Chess();
        dad = new DragAndDrop(PickObject, DropObject);
        onPromotionMove = "";
    }
    void Start()
    {
        InitGameObjects();
        ShowFigures();
        ShowPromotionFigures();
    }
    void InitGameObjects()
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                string key = "" + x + y;
                string name = (x + y) % 2 == 0 ? "BlackSquare" : "WhiteSquare";
                squares[key] = CreateGameObject(name, x, y);
                figures[key] = CreateGameObject("p", x, y);
            }
        promots["Q"] = CreateGameObject("Q", 2, 8);
        promots["R"] = CreateGameObject("R", 3, 8);
        promots["B"] = CreateGameObject("B", 4, 8);
        promots["N"] = CreateGameObject("N", 5, 8);
        promots["q"] = CreateGameObject("q", 2, -1);
        promots["r"] = CreateGameObject("r", 3, -1);
        promots["b"] = CreateGameObject("b", 4, -1);
        promots["n"] = CreateGameObject("n", 5, -1);
    }
    void ShowPromotionFigures(string pawn = "")
    {
        foreach (GameObject pro in promots.Values)
            SetSprite(pro, ".");
        if (pawn == "P")
        {
            SetSprite(promots["Q"], "Q");
            SetSprite(promots["R"], "R");
            SetSprite(promots["B"], "B");
            SetSprite(promots["N"], "N");
        }
        if (pawn == "p")
        {
            SetSprite(promots["q"], "q");
            SetSprite(promots["r"], "r");
            SetSprite(promots["b"], "b");
            SetSprite(promots["n"], "n");
        }
    }
    GameObject CreateGameObject(string pattern, int x, int y)
    {
        GameObject go = Instantiate(GameObject.Find(pattern));
        go.transform.position = new Vector2(x * 2, y * 2);
        go.name = pattern;
        return go;
    }
    void SetSprite(GameObject go, string source)
    {
        go.GetComponent<SpriteRenderer>().sprite =
                    GameObject.Find(source).GetComponent<SpriteRenderer>().sprite;
    }
    void ShowFigures()
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                string key = "" + x + y;
                string figure = chess.GetFigureAt(x, y).ToString();
                figures[key].transform.position = squares[key].transform.position;
                if (figures[key].name == figure)
                    continue;
                SetSprite(figures[key], figure);
                figures[key].name = figure;
            }
    }
    void Update()
    {
        dad.Action();
    }
    void DropObject(Vector2 from, Vector2 to)
    {
        Debug.Log(from + " " + to);
        string e2 = VectorToSquare(from);
        string e4 = VectorToSquare(to);
        string figure = chess.GetFigureAt(e2).ToString();
        string move = figure + e2 + e4;
        if (move.Length != 5)
            return;
        Debug.Log(move);
        if (figure == "P" && e4[1] == '8' ||
            figure == "p" && e4[1] == '1')
            if (chess.Move(move) != chess)
            {
                onPromotionMove = move;
                ShowPromotionFigures(figure);
                return;
            }
        chess = chess.Move(move);
        ShowFigures();
        UnmarkSquares();
    }
    string VectorToSquare(Vector2 vector)
    {
        int x = Convert.ToInt32(vector.x / 2);
        int y = Convert.ToInt32(vector.y / 2);
        if (x >= 0 && x <= 7 && y >= 0 && y <= 7)
            return ((char)('a' + x)).ToString() + (y + 1).ToString();
        return "";
    }
    void PickObject(Vector2 from)
    {
        if (onPromotionMove != "")
        {
            int x = Convert.ToInt32(from.x / 2);
            if (onPromotionMove[0] == 'P')
            {
                if (x == 2) onPromotionMove += "Q";
                if (x == 3) onPromotionMove += "R";
                if (x == 4) onPromotionMove += "B";
                if (x == 5) onPromotionMove += "N";
            }
            if (onPromotionMove[0] == 'p')
            {
                if (x == 2) onPromotionMove += "q";
                if (x == 3) onPromotionMove += "r";
                if (x == 4) onPromotionMove += "b";
                if (x == 5) onPromotionMove += "n";
            }
            chess = chess.Move(onPromotionMove);
            onPromotionMove = "";
            ShowFigures();
            ShowPromotionFigures();
            UnmarkSquares();
            return;
        }
        MarkSquaresTo(VectorToSquare(from));
    }
    void UnmarkSquares()
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                ShowSquare(x, y);
    }
    void MarkSquaresTo(string from)
    {
        UnmarkSquares();
        foreach (string move in chess.YieldValidMoves())
            if (from == move.Substring(1, 2))
                ShowSquare(move[3] - 'a', move[4] - '1', true);
    }
    void ShowSquare(int x, int y, bool marked = false)
    {
        string square = (x + y) % 2 == 0 ? "BlackSquare" : "WhiteSquare";
        if (marked) square += "Marked";
        SetSprite(squares["" + x + y], square);
    }
}
class DragAndDrop
{
    State state;
    GameObject item;
    Vector2 offset;
    Vector2 fromPosition;
    Vector2 toPosition;
    public delegate void dePickObject(Vector2 from);
    public delegate void deDropObject(Vector2 from, Vector2 to);
    dePickObject PickObject;
    deDropObject DropObject;
    public DragAndDrop(dePickObject PickObject, deDropObject DropObject)
    {
        this.PickObject = PickObject;
        this.DropObject = DropObject;
        item = null;
        state = State.none;
    }
    enum State
    {
        none,
        drag
    }
    public void Action()
    {
        switch (state)
        {
            case State.none:
                if (IsMouseButtonPressed())
                    PickUp();
                break;
            case State.drag:
                if (IsMouseButtonPressed())
                    Drag();
                else
                    Drop();
                break;
        }
    }
    bool IsMouseButtonPressed()
    {
        return Input.GetMouseButton(0);
    }
    void PickUp()
    {
        Vector2 clickPosition = GetClickPosition();
        Transform clickedItem = GetItemAt(clickPosition);
        if (clickedItem == null)
            return;
        state = State.drag;
        item = clickedItem.gameObject;
        fromPosition = clickedItem.position;
        offset = fromPosition - clickPosition;
        PickObject(fromPosition);

    }
    Vector2 GetClickPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    Transform GetItemAt(Vector2 position)
    {
        RaycastHit2D[] figures = Physics2D.RaycastAll(position, position, 0.5f);
        if (figures.Length == 0)
            return null;
        return figures[0].transform;
    }
    void Drag()
    {
        item.transform.position = GetClickPosition() + offset;
    }
    void Drop()
    {
        toPosition = item.transform.position;
        DropObject(fromPosition, toPosition);
        item = null;
        state = State.none;
    }
}