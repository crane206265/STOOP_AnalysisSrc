import numpy as np
import matplotlib.pyplot as plt
import csv
import os

file_folder_path = "C:/Users/dlgkr/OneDrive/Desktop/code/astronomy/STOOP/Meridian_Passing_Data/"
file_list = [f for f in os.listdir(file_folder_path) if f.endswith('.csv')]

# -------------------- Function to Read CSV Data --------------------
def read_csv_data(file_path):
    ra_arr = np.array([])
    dec_arr = np.array([])

    with open(file_path, 'r') as file:
        reader = csv.reader(file)
        reader.__next__()
        for row in reader:
            if row:  # Check if the row is not empty
                ra_temp = float(row[1])
                dec_temp = float(row[2])
                ra_arr = np.append(ra_arr, ra_temp)
                dec_arr = np.append(dec_arr, dec_temp)

    return ra_arr, dec_arr

# -------------------- Function to Change the Range of Data --------------------
def change_range(ra_arr, dec_arr):
    ra_arr = ra_arr % 360
    dec_arr = dec_arr
    return ra_arr, dec_arr

# -------------------- Function to Plot the Path --------------------
def path_plot(ra_arr, dec_arr, title=None, resol=10):
    fig = plt.figure(figsize=(10, 3.5))
    ax1 = fig.add_subplot(1, 2, 1)
    ax2 = fig.add_subplot(1, 2, 2)

    ax1.plot(ra_arr, dec_arr, color='orangered', marker='o', markersize=0.5, linewidth=0.3, linestyle='dashed')
    ax1.plot(ra_arr[0], dec_arr[0], color='royalblue', marker='X', markersize=2, label='Start Point')
    ax1.plot(ra_arr[-1], dec_arr[-1], color='gold', marker='X', markersize=2, label='End Point')
    ax1.set_xlim(0, 360)
    ax1.set_ylim(-90, 90)
    ax1.grid()
    if title is not None: plt.title(title)
    ax1.legend()
    ax1.set_xlabel('Right Ascension (degrees)')
    ax1.set_ylabel('Declination (degrees)')

    # ---------- Plotting the Slope ----------
    slope_arr, null_idx = slope(ra_arr, dec_arr)
    ax2.plot(np.arange(0, ra_arr.shape[0]-1)[null_idx], slope_arr, color='orangered', linestyle='solid')
    ax2.plot(np.arange(0, ra_arr.shape[0]-1)[np.logical_not(null_idx)], np.zeros_like(np.arange(0, ra_arr.shape[0]-1)[np.logical_not(null_idx)]), color='gray', marker='x', linestyle='None', markersize=0.1, label='Inf')
    ax2.set_title('Slope of Path ($\Delta Dec / \Delta RA$)')
    ax2.legend()

    plt.show()

# -------------------- Function to Calculate the Slope --------------------
def slope(ra_arr, dec_arr):
    # calculate the slope by diff
    ra_diff = np.diff(ra_arr)
    null_idx = np.abs(ra_diff) > 5e-2
    return np.diff(dec_arr)[null_idx] / ra_diff[null_idx], null_idx
    
# -------------------- Main Code --------------------
                
for i in range(len(file_list)):
    ra_arr, dec_arr = read_csv_data(file_folder_path+file_list[i])
    ra_arr, dec_arr = change_range(ra_arr, dec_arr)
    
    print("Data from file : ", file_list[i])
    print("Data length : ", len(ra_arr))

    path_plot(ra_arr, dec_arr, title="Path of "+file_list[i], resol=10)

    print("Start Point : ", ra_arr[0], dec_arr[0])
    print("End Point : ", ra_arr[-1], dec_arr[-1])
    print("---------------------")
