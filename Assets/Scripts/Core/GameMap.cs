using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameMap : MonoBehaviour {
    [Header("Info")]
    public string id;
    public Sprite thumbnail;
    public string displayName;
    public string author;
    public string description;
    [Header("Data")]
    public Transform planeStartPosition;
    public List<Enemy> enemyPalette;
    public List<Area> areaPalette;
    public List<string> actionStrings;

    [HideInInspector] public string failedText;
    [HideInInspector] public List<MapAction> actions;
    private void Awake() {
        actions = new List<MapAction>(actionStrings.Count);
        for (int i = 0; i < actionStrings.Count; i++) {
            actions.Add(new MapAction(this, actionStrings[i]));
        }
    }
}

public class MapAction {
    public GameMap gameMap;
    public MapActionType type;
    public List<object> param;

    public MapAction(GameMap map, string line) {
        gameMap = map;
        string[] split = line.Trim().Split('|'); //|으로 나누기
        type = MapActionType.Of(split[0]); //0번째 인자로 종류 얻어오기
        if (type == null) { return; } //종류 유효 검사
        int paramLength = split.Length - 1;
        if (paramLength < type.minimumParamLength) { return; } //인자 유효 검사
        param = new List<object>(new object[paramLength]);
        string[] args = new string[paramLength];
        Array.Copy(split, 1, args, 0, paramLength); //0번째 인자 자르고 전달
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < args.Length; i++) builder.Append(args[i]).Append(" ");
        if (type.Parse == null) {
            return;
        }
        if (!type.Parse(map, param, args)) {
            Debug.Log("Cannot Parse Action " + line);
        }
    }

    public void Execute(GameController controller) {
        type.Execute?.Invoke(this, controller);
    }
}

public enum ActionEnum {
    NONE,
    WAIT,

    WAITFOR_TAKEOFF,
    WAITFOR_ENEMY_KILL,
    WAITFOR_ENTER_AREA,

    CREATE_ENEMY,
    DESTROY_ENEMY,
    DESTROY_ALL_ENEMY,

    ENABLE_AREA,
    DISABLE_AREA,

    RADIO,

    END_SCREEN_DESCRIPTION,
    CLEAR
}

//enum의 특징을 가진 클래스가 필요했음
//참조 https://docs.microsoft.com/ko-kr/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types
//위와 같이 public static readonly 필드를 열거형 객체로서 사용하는 클래스 Enumeration을 Util.cs에 구현해둠
//클래스이지만 열거형으로 동작함
public class MapActionType : Enumeration {

    //해석할 수 없는 구문 등
    public static readonly MapActionType NONE = new MapActionType("NONE", ActionEnum.NONE, 0, defaultExecute);

    //대기
    //WAIT|<float:seconds>
    //WAIT|1
    public static readonly MapActionType WAIT = new MapActionType("WAIT", ActionEnum.WAIT, 1,
    (map, param, args) => {
        if (!float.TryParse(args[0], out float seconds)) return false;
        param[0] = seconds;
        return true;
    },
    (action, controller) => {
        controller.Wait((float)action.param[0]);
    }
    );

    //이륙 시 까지 대기
    //altitude: 이륙 판정 최소 고도
    //WAITFOR_TAKEOFF|<float:altitude>
    //WAITFOR_TAKEOFF|10
    public static readonly MapActionType WAITFOR_TAKEOFF = new MapActionType("WAITFOR_TAKEOFF", ActionEnum.WAITFOR_TAKEOFF, 1,
    (map, param, args) => {
        if (!float.TryParse(args[0], out float altitude)) return false;
        param[0] = altitude;
        return true;
    },
    (action, controller) => {
        controller.WaitFor(controller.WaitForTakeOff((float)action.param[0]));
    }
    );

    //적기 사살 시 까지 대기
    //특정 group의 적기가 전부 파괴된 경우 넘어감
    //WAITFOR_ENEMY_KILL|<string:group>
    //WAITFOR_ENEMY_KILL|phase1
    public static readonly MapActionType WAITFOR_ENEMY_KILL = new MapActionType("WAITFOR_ENEMY_KILL", ActionEnum.WAITFOR_ENEMY_KILL, 1,
    (map, param, args) => {
        param[0] = args[0];
        return true;
    },
    (action, controller) => {
        controller.WaitFor(controller.WaitForEnemyKill((string)action.param[0]));
    }
    );

    //구역 진입 시 까지 대기
    //WAITFOR_ENTER_AREA|<int:AreaPalette Index>
    //WAITFOR_ENTER_AREA|0
    public static readonly MapActionType WAITFOR_ENTER_AREA = new MapActionType("WAITFOR_ENTER_AREA", ActionEnum.WAITFOR_ENTER_AREA, 1,
    (map, param, args) => {
        if (!int.TryParse(args[0], out int index)) return false; //palette index
        if (index < 0 || map.areaPalette.Count <= index) return false;
        param[0] = index;
        return true;
    },
    (action, controller) => {
        controller.WaitFor(controller.WaitForEnterArea((int)action.param[0]));
    }
    );

    //적기 생성
    //GameMap의 enemyPalette의 index번째의 Enemy를 해당 좌표에 Instantiate
    //group을 작성하지 않으면 default 그룹으로 들어감
    //CREATE_ENEMY|<int:EnemyPalette Index>|<float:posX>|<float:posY>|<float:posZ>|<float:yaw>|[|<string:group=default>]
    //CREATE_ENEMY|0|1000|80|1000|phase1
    public static readonly MapActionType CREATE_ENEMY = new MapActionType("CREATE_ENEMY", ActionEnum.CREATE_ENEMY, 5,
    (map, param, args) => {
        if (!int.TryParse(args[0], out int index)) return false; //palette index
        //맵의 적기 팔레트 범위보다 벗어난 경우
        if (index < 0 || map.enemyPalette.Count <= index) return false;
        param[0] = index;
        for (int i = 1; i <= 3; i++) //x, y, z 위치
        {
            if (!float.TryParse(args[i], out float f)) return false;
            param[i] = f;
        }
        if (!float.TryParse(args[4], out float yaw)) return false;
        param[4] = yaw;
        if (args.Length == 6) {
            param[5] = args[5]; //group
        }
        return true;
    },
    (action, controller) => {
        if (action.param.Count == 5) {
            controller.GenerateEnemy(
                (int)action.param[0],
                (float)action.param[1],
                (float)action.param[2],
                (float)action.param[3],
                (float)action.param[4]
            );
            return;
        }
        if (action.param.Count == 6) {
            controller.GenerateEnemy(
                (int)action.param[0],
                (float)action.param[1],
                (float)action.param[2],
                (float)action.param[3],
                (float)action.param[4],
                (string)action.param[5]
            );
        }
    }
    );

    //특정 그룹 파괴
    //DESTROY_ENEMY
    public static readonly MapActionType DESTROY_ENEMY = new MapActionType("DESTROY_ENEMY", ActionEnum.DESTROY_ENEMY, 1,
    (map, param, args) => {
        param[0] = args[0];
        return true;
    },
    (action, controller) => {
        controller.DestroyEnemy((string)action.param[0]);
    }
    );

    //모든 적기 파괴
    //DESTROY_ALL_ENEMY
    public static readonly MapActionType DESTROY_ALL_ENEMY = new MapActionType("DESTROY_ALL_ENEMY", ActionEnum.DESTROY_ALL_ENEMY, 0,
    (action, controller) => {
        controller.DestroyAllEnemy();
    }
    );

    //구역 활성화
    //ENABLE_AREA|<int:AreaPalette Index>
    //ENABLE_AREA|0
    public static readonly MapActionType ENABLE_AREA = new MapActionType("ENABLE_AREA", ActionEnum.ENABLE_AREA, 1,
    (map, param, args) => {
        if (!int.TryParse(args[0], out int index)) return false; //palette index
        if (index < 0 || map.areaPalette.Count <= index) return false;
        param[0] = index;
        return true;
    },
    (action, controller) => {
        controller.EnableArea((int)action.param[0]);
    }
    );

    //구역 비활성화
    //DISABLE_AREA|<int:AreaPalette Index>
    //DISABLE_AREA|0
    public static readonly MapActionType DISABLE_AREA = new MapActionType("DISABLE_AREA", ActionEnum.DISABLE_AREA, 1,
    (map, param, args) => {
        if (!int.TryParse(args[0], out int index)) return false; //palette index
        if (index < 0 || map.areaPalette.Count <= index) return false;
        param[0] = index;
        return true;
    },
    (action, controller) => {
        controller.DisableArea((int)action.param[0]);
    }
    );

    //무전
    //RADIO|<string:name>|<string:text>|<float:duration>
    //RADIO|0
    public static readonly MapActionType RADIO = new MapActionType("RADIO", ActionEnum.RADIO, 3,
    (map, param, args) => {
        param[0] = args[0];
        param[1] = args[1];
        if (!float.TryParse(args[2], out float duration)) return false;
        param[2] = duration;
        return true;
    },
    (action, controller) => {
        controller.Radio((string)action.param[0], (string)action.param[1], (float)action.param[2]);
    }
    );

    //게임 클리어
    //CLEAR
    public static readonly MapActionType CLEAR = new MapActionType("CLEAR", ActionEnum.CLEAR, 0,
    (action, controller) => {
        controller.GameEnd(true);
    }
    );

    //게임 종료 시 설명 텍스트
    //END_SCREEN_DESCRIPTION|<string:text>
    //END_SCREEN_DESCRIPTION|Failed to landing.\nYour aeroplane has been destroyed.
    public static readonly MapActionType END_SCREEN_DESCRIPTION = new MapActionType("END_SCREEN_DESCRIPTION", ActionEnum.END_SCREEN_DESCRIPTION, 1,
    (map, param, args) => {
        param[0] = args[0];
        return true;
    },
    (action, controller) => {
        controller.gameEndScreen.SetDescription((string)action.param[0]);
    }
    );

    //////////////////////////
    //////////////////////////
    //////////////////////////

    public int minimumParamLength { get; private set; }

    //구문 해석 시 - 받은 문자열을 param에 형식에 맞춰서 넣음
    public delegate bool ParseFunc(GameMap map, List<object> param, string[] args);
    public ParseFunc Parse { get; private set; }
    private static ParseFunc defaultParse = (map, param, args) => { return true; };

    //구문 실행 시
    public delegate void ExecuteFunc(MapAction action, GameController controller);
    public ExecuteFunc Execute { get; private set; }
    private static ExecuteFunc defaultExecute = (action, controller) => { return; };

    private MapActionType(string name, ActionEnum enumId, int paramLength, ExecuteFunc executeFunc) : this(name, enumId, paramLength, defaultParse, executeFunc) { }
    private MapActionType(string name, ActionEnum enumId, int paramLength, ParseFunc parseFunc, ExecuteFunc executeFunc) : base((int)enumId, name) {
        Parse = parseFunc;
        Execute = executeFunc;
        minimumParamLength = paramLength;
    }
    public static MapActionType Of(string name) {
        var values = GetAll<MapActionType>();
        foreach (var type in values) {
            if (type.Name.Equals(name)) return type;
        }
        return NONE;
    }
}
