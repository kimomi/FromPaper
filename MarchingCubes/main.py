# marching cubs 算法
import numpy as np
import vector3
import cube

height = 100
x_size = height
y_size = height
size = 0.01

### 生成一个球形的数据
def generate_sphere_data(center, radius): 
    result = np.zeros((height, x_size, y_size))
    for i in range(0, height):
        for j in range(0, x_size):
            for k in range(0, y_size):
                point_pos = vector3.vector3(j * size, k * size, i * size)
                if ((point_pos - center).sqrmagnitude() >= radius * radius):
                    result[i, j, k] = 0
                else:
                    result[i, j, k] = 1
    return result

# marching cubes主要算法
def marching_cubes(origin_data, value_data):
    verticles_list = []
    for cur_index in range(0, origin_data.shape[0] - 1):
        # 一次读取4片切片
        slice0 = origin_data[cur_index]
        slice1 = origin_data[cur_index + 1]

        # 从两张切片中读取出一个方块
        for x in range(0, slice0.shape[0] - 1):
            for y in range(0, slice0.shape[1] - 1):
                cube_data = cube.cube(slice0[x, y], slice1[x, y], slice1[x + 1, y], slice0[x + 1, y],\
                    slice0[x, y + 1], slice1[x, y + 1], slice1[x + 1, y + 1], slice0[x + 1, y + 1])
                for v in cube_data.get_vertex_list(value_data):
                    verticles_list.append(vector3.vector3(x + v[0], y + v[1], cur_index + v[2]) * size)
    return verticles_list



if __name__ == "__main__":
    list_data = generate_sphere_data(vector3.vector3(0.5, 0.5, 0.5), 0.3)
    result = marching_cubes(list_data, 0.5)
    # 打开一个文件
    fo = open("my_vertex.txt", "w")
    fo.write( "#MiniLight\n1\n1024 578\n")
    fo.write("(0.5 0.5 -4) (0 0 1) 60\n(0.8 0.8 0.8) (0.1 0.1 0.1)\n")
    
    index = 0
    for i in result:
        index = index + 1
        fo.write(str(i) + " ")
        if (index % 3 == 0):
            fo.write("(0.7 0.7 0.7) (0 0 0)\n")
    
    fo.flush()
    fo.close()