# DragControl
通过自定义一个Helper类实现控件随意拖拽、调整大小。
### 使用
使用方法很简单，Canvas长宽绑定显示区，内嵌Grid与DragControlHelper，Grid中再使用Viewbox完成缩放即可。
```xml
        <Canvas x:Name="brc" Width="{Binding ElementName=win, Path=Width}" Height="{Binding ElementName=win, Path=Height}">
            <Grid Background="Transparent" Canvas.Top="298" Canvas.Left="502" local:DragControlHelper.IsEditable="True" local:DragControlHelper.IsSelectable="True">
                <Viewbox IsHitTestVisible="False">
                    <local:Legend x:Name="br" Height="152" Width="298"/>
                </Viewbox>
            </Grid>
            <local:DragControlHelper/>
        </Canvas>
```
### 实现效果
单击选中状态，边缘8个小点实现8个方向的拖拽，拖拽中间实现拖动。

![DragControl效果](https://github.com/GatoNaranja/Images/blob/main/DragControl.PNG)
