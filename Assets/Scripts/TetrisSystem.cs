using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisSystem : MonoBehaviour
{
    const int FIELD_SIZE_X = 12;
    const int FIELD_SIZE_Y = 22;
    const int MOVE_SIZE_X = 5;
    const int MOVE_SIZE_Y = 5;
    const int DEFAULT_MOVE_X = 3;
    const int DEFAULT_MOVE_Y = 15;
    [SerializeField] GameObject _blockPrefab = null;
    //ブロックの状態
    public enum eBlockState
    {
        eNone,
        eFrame,
        eSkyBlue,
        eYellow,
        ePurple,
        eBlue,
        eOrange,
        eGreen,
        eRed,

        eMax
    }

    static readonly int[,] BLOCKS_SKYBLUE = new int[MOVE_SIZE_X, MOVE_SIZE_Y]
    {
        {0,0,0,0,0,},
        {2,2,2,2,0,},
        {0,0,0,0,0,},
        {0,0,0,0,0,},
        {0,0,0,0,0,},
    };
   
    static readonly int[,] BLOCKS_YELLOW = new int[MOVE_SIZE_X, MOVE_SIZE_Y]
    {
        {0,0,0,0,0,},
        {0,3,3,0,0,},
        {0,3,3,0,0,},
        {0,0,0,0,0,},
        {0,0,0,0,0,},
    };

     static readonly int[,] BLOCKS_PURPLE = new int[MOVE_SIZE_X, MOVE_SIZE_Y]
    {
        {0,0,0,0,0,},
        {0,0,4,0,0,},
        {0,4,4,4,0,},
        {0,0,0,0,0,},
        {0,0,0,0,0,},
    };

     static readonly int[,] BLOCKS_BLUE = new int[MOVE_SIZE_X, MOVE_SIZE_Y]
    {
        {0,0,0,0,0,},
        {0,5,0,0,0,},
        {0,5,5,5,0,},
        {0,0,0,0,0,},
        {0,0,0,0,0,},
    };
     static readonly int[,] BLOCKS_ORANGE = new int[MOVE_SIZE_X, MOVE_SIZE_Y]
    {
        {0,0,0,0,0,},
        {0,0,0,6,0,},
        {0,6,6,6,0,},
        {0,0,0,0,0,},
        {0,0,0,0,0,},
    };
     static readonly int[,] BLOCKS_GREEN = new int[MOVE_SIZE_X, MOVE_SIZE_Y]
    {
        {0,0,0,0,0,},
        {0,0,7,7,0,},
        {0,7,7,0,0,},
        {0,0,0,0,0,},
        {0,0,0,0,0,},
    };
       static readonly int[,] BLOCKS_RED = new int[MOVE_SIZE_X, MOVE_SIZE_Y]
    {
        {0,0,0,0,0,},
        {0,8,8,0,0,},
        {0,0,8,8,0,},
        {0,0,0,0,0,},
        {0,0,0,0,0,},
    };

    static readonly int[][,] BLOCKS_LIST = new int [(int)eBlockState.eMax - (int)eBlockState.eSkyBlue][,]
    {
        BLOCKS_SKYBLUE,
        BLOCKS_YELLOW,
        BLOCKS_PURPLE,
        BLOCKS_BLUE,
        BLOCKS_ORANGE,
        BLOCKS_GREEN,
        BLOCKS_RED,
    };
    //ブロックの実体
    private Block[,] _fieldBlocks = new Block[FIELD_SIZE_Y,FIELD_SIZE_X];
    private GameObject[,] _fieldBlocksObject = new GameObject[FIELD_SIZE_Y,FIELD_SIZE_X];
    //フィールド上のブロックの状態
    private eBlockState [,] _fieldBlocksState = new eBlockState[FIELD_SIZE_Y,FIELD_SIZE_X];
    //最終的なのブロックの状態
    private eBlockState [,] _fieldBlocksStateFinal = new eBlockState[FIELD_SIZE_Y,FIELD_SIZE_X];
    //動作中ブロック
    private eBlockState[,] _moveBlocksState = new eBlockState[MOVE_SIZE_Y,MOVE_SIZE_X];
    private int _moveBlockX = DEFAULT_MOVE_X;
    private int _moveBlockY = DEFAULT_MOVE_Y;
    private eBlockState[,] _tempBlocksState = new eBlockState[MOVE_SIZE_Y,MOVE_SIZE_X];
    //落下時間間隔
    private float _fallTime = 1.0f;
    private float _fallTimer = 1.0f;
    //キー入力
    private Dictionary<KeyCode, int> _keyInputTimer = new Dictionary<KeyCode, int>();
    private bool GetKeyEx(KeyCode keyCode)
    {
        if (!_keyInputTimer.ContainsKey(keyCode))
        {
          _keyInputTimer.Add(keyCode, -1);  
        }
        if (Input.GetKey(keyCode))
        {
            _keyInputTimer[keyCode]++;
        }
        else
        {
            _keyInputTimer[keyCode] = -1;
        }
        return (_keyInputTimer[keyCode] == 0 || _keyInputTimer[keyCode] >= 10);
    }
    // Start is called before the first frame update


    void Start()
    {
        //初期状態の設定
        for (int i = 0; i < FIELD_SIZE_Y; i++){
            for (int j = 0; j< FIELD_SIZE_X; j++)
            {
                //ブロックの実体
            GameObject newObject = GameObject.Instantiate<GameObject>(_blockPrefab);
            Block newBlock = newObject.GetComponent<Block>();
           newObject.transform.localPosition = new Vector3(j,i,0.0f);
           　_fieldBlocksObject[i,j] = newObject;
            _fieldBlocks[i,j] = newBlock;
            //ブロックの状態
            _fieldBlocksState[i,j] = (0 < i && i < FIELD_SIZE_Y-1 && 0 < j && j < FIELD_SIZE_X-1) ? eBlockState.eNone : eBlockState.eFrame;
            newBlock.SetState(_fieldBlocksState[i,j]);
            _fieldBlocksStateFinal[i,j] = _fieldBlocksState[i,j];
             }
        }
       StartMove();
    }

    // Update is called once per frame
    void Update()
    {
        //ブロックを左右移動させる
        if (GetKeyEx(KeyCode.S))
        {
            //ブロックの当たり判定（左）
            bool isCollision = CheckCollision(-1,0);
            if (!isCollision)
            {
                _moveBlockX--;
            }
        }
        //ブロックの右回転
        if (GetKeyEx(KeyCode.L))
        {
            bool isCollision = CheckCollisionRotateRight();
            if (!isCollision)
            {
                RotateBlockRight();
            }

        } if (GetKeyEx(KeyCode.J))
        {
            bool isCollision = CheckCollisionRotateLeft();
            if (!isCollision)
            {
                RotateBlockLeft();
            }

        }
        if (GetKeyEx(KeyCode.F))
        {
            //ブロックの当たり判定（右）
            bool isCollision = CheckCollision(1,0);
            if (!isCollision)
            {
                _moveBlockX++;
            }
        }
        //ブロックを落下させる
        _fallTimer -= Time.deltaTime;
        if (_fallTimer <= 0.0f || GetKeyEx(KeyCode.D))
        {
            //ブロックの当たり判定（下）
            bool isCollision = CheckCollision(0,-1); 
            if (isCollision)
            { //ブロックの落下を反映
                MergeBlock();
                //ブロック揃いをチェック
                CheckLine();
                CheckLine();
                CheckLine();
                CheckLine();
                //タイマーをリセット
        _fallTimer = _fallTime;
        StartMove();
            } else {
        //ブロックを落下
        _moveBlockY--;
        //タイマーをリセット
        _fallTimer = _fallTime;
        }
        }
        
        //ブロックの状態を更新
        UpdateBlockState();
    }

    //ブロックの回転処理
    void CacheTempState()
    {
        for(int i = 0; i < MOVE_SIZE_Y; i++)
            {
                for(int j = 0; j < MOVE_SIZE_X; j++)
                {
                    _tempBlocksState[i,j] = _moveBlocksState[i,j];
                }
            }
    }
    void RotateBlockRight()
        {
            CacheTempState();
            for(int i = 0; i < MOVE_SIZE_Y; i++)
            {
                for(int j = 0; j < MOVE_SIZE_X; j++)
                {
                     _moveBlocksState[i,j] = _tempBlocksState[j,MOVE_SIZE_Y -1 - i];
                }
            }
            
        }
    void RotateBlockLeft()
        {
            CacheTempState();
            for(int i = 0; i < MOVE_SIZE_Y; i++)
            {
                for(int j = 0; j < MOVE_SIZE_X; j++)
                {
                     _moveBlocksState[i,j] = _tempBlocksState[MOVE_SIZE_X -1 - j,i];
                }
            }
            
        }
    bool CheckCollision(int offsetX, int offsetY)
    {
 for (int i = 0; i < MOVE_SIZE_Y; i++)
        {
            for (int j = 0; j< MOVE_SIZE_X; j++)
            {
                if (0 <= _moveBlockY + i + offsetY&& _moveBlockY + i + offsetY < FIELD_SIZE_Y && 0 <= _moveBlockX + j + offsetX && _moveBlockX + j + offsetX < FIELD_SIZE_X)
                {
                //ブロックの状態
             if (_fieldBlocksState[_moveBlockY + i + offsetY, _moveBlockX + j + offsetX] != eBlockState.eNone && _moveBlocksState[i,j] != eBlockState.eNone) {
                 return true;
             }
                }
            }
        }
        return false;
    }
    //ブロックの当たり判定（回転）
     bool CheckCollisionRotate()
    {
 for (int i = 0; i < MOVE_SIZE_Y; i++)
        {
            for (int j = 0; j< MOVE_SIZE_X; j++)
            {
                if (0 <= _moveBlockY + i && _moveBlockY + i < FIELD_SIZE_Y && 0 <= _moveBlockX + j && _moveBlockX + j < FIELD_SIZE_X)
                {
                //ブロックの状態
             if (_fieldBlocksState[_moveBlockY + i , _moveBlockX + j ] != eBlockState.eNone && _tempBlocksState[i,j] != eBlockState.eNone) {
                 return true;
             }
                }
            }
        }
        return false;
    }
     bool CheckCollisionRotateLeft()
    {
 for (int i = 0; i < MOVE_SIZE_Y; i++)
        {
            for (int j = 0; j< MOVE_SIZE_X; j++)
            {
                _tempBlocksState[i,j] = _moveBlocksState[MOVE_SIZE_X-1-j,i];
            }
        }
        return CheckCollisionRotate();
    }
     bool CheckCollisionRotateRight()
    {
 for (int i = 0; i < MOVE_SIZE_Y; i++)
        {
            for (int j = 0; j< MOVE_SIZE_X; j++)
            {
                _tempBlocksState[i,j] = _moveBlocksState[j,MOVE_SIZE_Y-1-i];
            }
        }
        return CheckCollisionRotate();
    }
  
    　//ブロックの状態を更新
    void UpdateBlockState()
    {
//ブロックの状態反映(フィールド上)
        for (int i = 0; i < FIELD_SIZE_Y; i++)
        {
            for (int j = 0; j< FIELD_SIZE_X; j++)
            {
                //ブロックの状態
             _fieldBlocksStateFinal[i,j] = _fieldBlocksState[i,j];
            }
        }
          //ブロックの状態反映（動作中）
        for (int i = 0; i < MOVE_SIZE_Y; i++)
        {
            for (int j = 0; j< MOVE_SIZE_X; j++)
            {
                //ブロックの状態
                if (0 <= _moveBlockY + i && _moveBlockY + i < FIELD_SIZE_Y && 0 <= _moveBlockX + j && _moveBlockX + j < FIELD_SIZE_X)
                {
                    if (_fieldBlocksStateFinal[_moveBlockY + i, _moveBlockX+ j] == eBlockState.eNone)
                    {
                        _fieldBlocksStateFinal[_moveBlockY+i,_moveBlockX+j] = _moveBlocksState[i,j];
                    }
                }
            }
        }
         //ブロックの状態反映
        for (int i = 0; i < FIELD_SIZE_Y; i++)
        {
            for (int j = 0; j< FIELD_SIZE_X; j++)
            {
                //ブロックの状態
             _fieldBlocks[i,j].SetState(_fieldBlocksStateFinal[i,j]);
            }
        }
    }

    void MergeBlock()
    {  
            //ブロックの状態反映
        for (int i = 0; i < MOVE_SIZE_Y; i++)
        {
            for (int j = 0; j< MOVE_SIZE_X; j++)
            {
                if (0 <= _moveBlockY + i && _moveBlockY + i < FIELD_SIZE_Y && 0 <= _moveBlockX + j && _moveBlockX + j < FIELD_SIZE_X)
                {
                    if (_fieldBlocksState[_moveBlockY+i,_moveBlockX+j] == eBlockState.eNone)
                    {
                //ブロックの状態
             _fieldBlocksState[_moveBlockY+i,_moveBlockX+j] = _moveBlocksState[i,j];
                    }
                }
            }
        }
    }
    void CheckLine()
    {
        for (int i = 1; i < FIELD_SIZE_Y-1; i++)
        {
            bool isBlank = false;
            for (int j = 1; j< FIELD_SIZE_X-1; j++)
            {
                    if (_fieldBlocksState[i,j] == eBlockState.eNone)
                    {
                //ブロックの状態
                    isBlank = true;
                    }   
            }
            if (!isBlank)
            {
                DeleteLine(i);
            }
        }
    }
    void DeleteLine(int y)
    {
        for (int i = y; i < FIELD_SIZE_Y-1; i++)
        {
            for (int j = 1; j< FIELD_SIZE_X-1; j++)
            {
                if (_fieldBlocksState[i,j] >= eBlockState.eSkyBlue)
                {
                 _fieldBlocksState[i,j] = _fieldBlocksState[i+1, j];
                }
            }
    
        }
    }
    //ブロックを開始
    void StartMove()
    {
        //初期位置を設定
        _moveBlockX = DEFAULT_MOVE_X;
        _moveBlockY = DEFAULT_MOVE_Y;

        //ランダム
        int pattern = Random.Range(0,eBlockState.eMax-eBlockState.eSkyBlue);
        int[,] blocks = BLOCKS_LIST[pattern];
        for (int i = 0; i < MOVE_SIZE_Y; i++)
        {
            for (int j = 0; j< MOVE_SIZE_X; j++)
            {
                //ブロックの状態
             _moveBlocksState[i,j] = (eBlockState)blocks[i,j];
            }
        }
    }
}
