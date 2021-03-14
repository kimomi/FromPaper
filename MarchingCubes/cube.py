import look_up_table
import numpy as np

class cube:
    def __init__(self, v0, v1, v2, v3, v4, v5, v6, v7):
        self.v = np.array([v0, v1, v2, v3, v4, v5, v6, v7]) 

    def get_vertex_list(self, evalute_value):
        index = 0
        result = []
        for i in range(0, 8):
            if (self.v[i] >= evalute_value):
                index |= 1 << i

        edges = look_up_table.triangleTableBroken[index]
        for i in range(1, 16):
            if edges[i] >= 0:
                edge = look_up_table.edgeTable[edges[i]]
                v1 = look_up_table.vertPos[edge[0]]
                v2 = look_up_table.vertPos[edge[1]]
                result.append((v1 + v2) / 2)

        return result