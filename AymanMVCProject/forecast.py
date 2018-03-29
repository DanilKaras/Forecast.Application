#!/usr/bin/env python
import sys
import pandas as pd 
import matplotlib.pyplot as plt
import matplotlib as mpl
import matplotlib.dates as mdates
import numpy
import matplotlib
from matplotlib import pylab, mlab, pyplot
from IPython.core.pylabtools import figsize, getfigs

from fbprophet import Prophet
from datetime import datetime 
from datetime import timedelta 

path = sys.argv[1]
pers = int(sys.argv[2])
seasonalityHourly = (sys.argv[3] == 'True') 
seasonalityDaily = (sys.argv[4] == 'True') 

# #plt = pyplot
plt.style.use('ggplot')
pyplot.rcParams['figure.figsize'] = (10, 6)

pd.options.display.float_format = '{:,.10f}'.format

df = pd.read_csv(path + '/data.csv', parse_dates=['Time'])

df.set_index('Time', inplace=True)
pd.set_option('display.height', None)
pd.set_option('display.max_rows', None)

df['ds'] = df.index
df['y'] = df['avg']
forecast_data = df[['ds', 'y']].copy()
forecast_data.reset_index(inplace=True)
del forecast_data['Time']

m = Prophet()
plotPos = 1
if seasonalityHourly:
    m.add_seasonality(name='hourly', period=0.04, fourier_order=5)
    plotPos = 2

if seasonalityDaily:
    m.add_seasonality(name='daily', period=1, fourier_order=5)

m.fit(forecast_data)

future = m.make_future_dataframe(periods = pers, freq='H')
forecast = m.predict(future)
fig = m.plot(forecast, xlabel='', ylabel='   ')
fig.gca().yaxis.set_major_formatter(mpl.ticker.StrMethodFormatter('{x:,.5f}'))


cmp = m.plot_components(forecast)
allAxes = cmp.get_axes()

subPlot = allAxes[plotPos].get_xticks()

newTicks = []
sub = (subPlot[-1]-subPlot[0])/24
start = subPlot[0]
for i in range(0,25):
    newTicks.append(start)
    start += sub


allAxes[plotPos].set_xticks(newTicks)
@mpl.ticker.FuncFormatter
def major_formatter(x, pos):
    if pos == 24:
        return 0
    return pos

formatter = mpl.ticker.FuncFormatter(mdates.DateFormatter('%m/%d/%y'))

allAxes[plotPos].xaxis.set_major_formatter(major_formatter)
allAxes[0].xaxis.set_major_formatter(formatter)

forecast.to_csv(path+'/out.csv')
fig.savefig(path+'/forecast.png')
cmp.savefig(path+'/components.png', dpi=200)