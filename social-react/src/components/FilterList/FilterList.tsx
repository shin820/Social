import * as React from 'react';
import { FilterListItem } from '../../types';

export interface Props {
    list: FilterListItem[],
    onReload?: () => void;
}

export default function FilterList({ list, onReload }: Props) {
    return <div>
        <div>Filters</div>
        <div>
            <ul>
                {list.map(filter => <li key={filter.id}>{filter.name} - {filter.unReadNum}</li>)}
            </ul>
            <button onClick={onReload}>reload</button>
        </div>
    </div>
}