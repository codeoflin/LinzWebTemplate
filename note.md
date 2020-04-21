# LinzWebTemplate

LinzWebTemplate

### 安装/卸载 Install/Uninstall
安装(更新也可用这个命令):
```shell
dotnet new -i 文件夹路径
```

Example:
```shell
dotnet new -i ./
```

卸载:
```shell
dotnet new -u ./
```

### 使用
新建工程:

|参数| 描述 |
|--- | --- |
| -N -NoPicture | 是否不使用模板图片,设为true则无图片 | 

```shell
dotnet new ocvc -n 工程名 参数...
```

Example:
```shell
dotnet new linzweb -n newproject -N true
```

### 参考文档 Documents
https://www.cnblogs.com/laozhang-is-phi/p/10205495.html
https://www.cnblogs.com/catcher1994/p/10061470.html
https://docs.microsoft.com/zh-cn/dotnet/core/tools/custom-templates