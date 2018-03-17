#!/usr/bin/env python
import sys
import pandas as pd 
import matplotlib.pyplot as plt
import matplotlib as mpl
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

if seasonalityHourly:
    m.add_seasonality(name='hourly', period=0.04, fourier_order=5)

if seasonalityDaily:
    m.add_seasonality(name='daily', period=1, fourier_order=5)

m.fit(forecast_data)

future = m.make_future_dataframe(periods = pers, freq='H')
forecast = m.predict(future)
fig = m.plot(forecast, xlabel='Date', ylabel='Price (BTC)')

fig.gca().yaxis.set_major_formatter(mpl.ticker.StrMethodFormatter('{x:,.10f}'))

forecast.to_csv(path+'/out.csv')
fig.savefig(path+'/forecast.png')
m.plot_components(forecast).savefig(path+'/components.png')
