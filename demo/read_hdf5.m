% 读取h5数据
data = h5read('test.h5','/IF1509_20150202');

% 字符串要转置再处理一下
data.Symbol = cellstr(data.Symbol');
data.Exchange = cellstr(data.Exchange');

t = struct2table(data);

% 先将int32转int64
t.datetime = int64(t.ActionDay)*1000000 + int64(t.UpdateTime);

% 可以再转存成csv
writetable(t,'t.csv');