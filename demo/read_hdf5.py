# -*- coding: utf-8 -*-

################
# 使用pandas来读取
import pandas as pd
df1 = pd.read_hdf(r'test.h5','IF1509_20150202')
df1.dtypes

################
# 使用h5py来读取
import h5py
f = h5py.File(r'test.h5', 'a')

# 打印表名
for name in f:
    print(name)

ds = f['IF1509_20150202'][:]

df2 = pd.DataFrame(ds)
df2.dtypes

################
# 时间处理,可看情况是否加入毫秒
import numpy as np
df2['datetime'] = df2['ActionDay'].astype(np.int64)*1000000+df2['UpdateTime']

