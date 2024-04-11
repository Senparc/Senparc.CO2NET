## Senparc.CO2NET.Cache.Dapr.Tests测试说明

先决条件：

- 测试用机上已安装Dapr且完成初始化。
- 测试用机上已安装.NET6版本的SDK。

- `~/.dapr/components`文件夹下包含Dapr初始化时自动创建的statestore组件。



测试步骤：

1. 在`~/.dapr/components`文件夹下创建`lockstore.yaml`文件，写入以下内容，用来测试分布式锁功能。

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: lockstore
spec:
  type: lock.redis
  metadata:
  - name: redisHost
    value: localhost:6379
  - name: redisPassword
    value: ""
```

2. 进入`Senparc.CO2NET.Cache.Dapr.Tests`项目所在目录，运行以下命令进行测试：

```
dapr run --app-id myapp --dapr-http-port 3500 dotnet test
```