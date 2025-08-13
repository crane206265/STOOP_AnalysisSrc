import numpy as np
import matplotlib.pyplot as plt

import csv

virgo_cluster_file_path = "C:/Users/dlgkr/Downloads/virgo_cluster_list.csv"
f = open(virgo_cluster_file_path, 'r')
reader = csv.reader(f)
header = next(reader)  # Skip header row

vir_name = []
vir_ra = []
vir_dec = []

for row in reader:
    vir_name.append(row[0])
    vir_ra.append(float(row[1]))
    vir_dec.append(float(row[2]))

vir_ra = np.radians(np.array(vir_ra))
vir_dec = np.radians(np.array(vir_dec))
        
f.close()

def dist(ra1, dec1, ra2, dec2):
    cosa = np.sin(dec1) * np.sin(dec2) + np.cos(dec1) * np.cos(dec2) * np.cos(ra1 - ra2)
    return np.arccos(cosa)


np.random.seed(72)

idx = np.random.randint(0, len(vir_name), 50)

star_name = ["Boo alpha", "Boo beta", "Boo epsilon", "Leo beta", "Leo alpha"]

star_ra = 15*np.array([
    14+15/60,
    15+1/60,
    14+44/60,
    11+49/60,
    10+8/60
])

star_dec = np.array([
    19+10/60,
    40+23/60,
    27+4/60,
    14+34/60,
    11+58/60
])

sel_vir_ra = vir_ra[idx]
sel_vir_dec = vir_dec[idx]

# delete Denebola
star_ra = star_ra[np.array([True, True, True, False, True])]
star_dec = star_dec[np.array([True, True, True, False, True])]

star_ra = star_ra * np.pi/180
star_dec = star_dec * np.pi/180

def pos_plot(labels=None):
    #plt.plot(sel_vir_ra*180/np.pi, sel_vir_dec*180/np.pi, 'o', label='Virgo Cluster', markersize=10, color=colors[0])
    #plt.plot(star_ra*180/np.pi, star_dec*180/np.pi, 'o', label='Stars', markersize=10)
    
    if labels is None:
        labels = np.zeros_like(total_ra)
        label_num = 1
    else: label_num = int(np.max(labels)+1)
    cmap = plt.get_cmap('tab20')
    colors = [cmap(i) for i in range(labels.shape[0])]

    for label in range(label_num):
        plt.plot(total_ra[labels == label]*180/np.pi, total_dec[labels == label]*180/np.pi, 'o', markersize=10, color=colors[label], label=str(label))
    plt.legend()
    plt.show()

N = len(sel_vir_ra) + len(star_ra)

total_ra = np.concatenate((sel_vir_ra, star_ra))
total_dec = np.concatenate((sel_vir_dec, star_dec))

# distance matrix
dists = np.zeros((N, N))
for i in range(N):
    for j in range(i):
        dists[i, j] = dist(total_ra[i], total_dec[i], total_ra[j], total_dec[j])
        dists[j, i] = dists[i, j]





def DBSCAN1(dists_matrix, eps=0.3):
    cluster_idx = -1
    N = dists_matrix.shape[0]
    included = -1 * np.ones((N))

    for i in range(N):
        if included[i] > -1: continue #cluster idx is already allocated
        cluster_idx += 1
        included[i] = cluster_idx + 0
        for j in range(N):
            if dists_matrix[i, j] <= eps:
                included[j] = cluster_idx + 0

    print(included)
    return included



label_group = DBSCAN1(dists, 0.1)

dists_nonrepeated = dists[np.triu_indices(N, k=1)]

print("-"*30)
print("Number of objects : "+str(N))
print("Mean distance : "+str(np.mean(dists_nonrepeated)))
print("Median distance : "+str(np.median(dists_nonrepeated)))
print("Standard deviation : "+str(np.std(dists_nonrepeated)))

if True:
    fig = plt.figure(figsize=(10, 3.5))
    ax1 = fig.add_subplot(1, 2, 1)
    ax2 = fig.add_subplot(1, 2, 2)

    
    dist_mat = ax1.imshow(dists)
    ax1.set_title("Distance Matrix")
    plt.colorbar(dist_mat, ax=ax1)

    ax2.hist(dists_nonrepeated.flatten(), bins=2*N)
    ax2.set_title("Distance Histogram")
    ax2.set_xlabel("Distance (radians)")
    ax2.set_ylabel("Density")
    plt.show()

pos_plot(label_group)



