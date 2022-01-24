# Bian.Controls
WinForm 自定义控件
- **SlideSwitch**

![dfa0cb2c6c870303a9f81b7c82f0665b.png](https://github.com/bianchh/Bian.Controls/blob/master/Controls/Images/dfa0cb2c6c870303a9f81b7c82f0665b.png)

控件的默认事件为 `SwitchChanged`
```C#
private void slideSwitch1_SwitchChanged(object sender, EventArgs e)
{
	SlideSwitch slideSwitch = sender as SlideSwitch;
	if (slideSwitch.IsOpen)
	{
		// to do ...		
	}
	else
	{
		// to do ...		
	}
}
```
- **PlaceHeaderTextBox**

![5fabdadc7109d5c2593b5be0d3dd3a24.png](https://github.com/bianchh/Bian.Controls/blob/master/Controls/Images/5fabdadc7109d5c2593b5be0d3dd3a24.png)

使用属性 `PlaceholderText` 可以更改提示的内容
